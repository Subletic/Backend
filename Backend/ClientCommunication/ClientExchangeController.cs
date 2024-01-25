namespace Backend.ClientCommunication;

using System.Net.WebSockets;
using System.Text;
using Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.FrontendCommunication;
using Backend.SpeechBubble;
using Backend.SpeechEngine;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

/// <summary>
/// The ClientExchangeController receives a transcription request from a client via a WebSocket
/// and returns the transcribed, corrected and converted substitles.
/// </summary>
[ApiController]
public class ClientExchangeController : ControllerBase
{
    /// <summary>
    /// Dependency Injection for accessing needed Services.
    /// </summary>
    private const int RECEIVE_BUFFER_SIZE = 8;

    private readonly IAvReceiverService avReceiverService;
    private readonly ISpeechBubbleListService speechBubbleListService;
    private readonly ISpeechmaticsConnectionService speechmaticsConnectionService;
    private readonly ISpeechmaticsReceiveService speechmaticsReceiveService;
    private readonly ISpeechmaticsSendService speechmaticsSendService;
    private readonly ISubtitleExporterService subtitleExporterService;
    private readonly IConfiguration configuration;
    private readonly ILogger log;
    private readonly IConfigurationService configurationService;
    private readonly IFrontendCommunicationService frontendCommunicationService;
    private static bool alreadyConnected = false;
    private TimeSpan clientTimeout;

    /// <summary>
    /// Constructor for ClientExchangeController.
    /// Gets instances of services via Dependency Injection.
    /// </summary>
    /// <param name="avReceiverService">The av receiver service.</param>
    /// <param name="speechBubbleListService">The speech bubble list service.</param>
    /// <param name="speechmaticsConnectionService">The speechmatics connection management service.</param>
    /// <param name="speechmaticsReceiveService">The speechmatics message receive service.</param>
    /// <param name="speechmaticsSendService">The speechmatics message send service.</param>
    /// <param name="subtitleExporterService">The subtitle exporter service.</param>
    /// <param name="configuration"> The configuration.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="log">The logger.</param>
    /// <param name="frontendCommunicationService">The Frontend Communication Service.</param>
    public ClientExchangeController(
        IAvReceiverService avReceiverService,
        ISpeechBubbleListService speechBubbleListService,
        ISpeechmaticsConnectionService speechmaticsConnectionService,
        ISpeechmaticsReceiveService speechmaticsReceiveService,
        ISpeechmaticsSendService speechmaticsSendService,
        ISubtitleExporterService subtitleExporterService,
        IConfiguration configuration,
        IConfigurationService configurationService,
        ILogger log,
        IFrontendCommunicationService frontendCommunicationService)
    {
        this.avReceiverService = avReceiverService;
        this.speechBubbleListService = speechBubbleListService;
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.speechmaticsReceiveService = speechmaticsReceiveService;
        this.speechmaticsSendService = speechmaticsSendService;
        this.subtitleExporterService = subtitleExporterService;
        this.configuration = configuration;
        this.configurationService = configurationService;
        this.log = log;
        this.frontendCommunicationService = frontendCommunicationService;
        clientTimeout =
            TimeSpan.FromSeconds(configuration.GetValue<double>("ClientCommunicationSettings:TIMEOUT_IN_SECONDS"));
    }

    private CancellationToken makeConnectionTimeoutToken()
    {
        return new CancellationTokenSource((int)clientTimeout.TotalMilliseconds).Token;
    }

    /// <summary>
    /// Represents an asynchronous operation that can return a value.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Route("/transcribe")]
    public async Task Get()
    {
        if (alreadyConnected)
        {
            log.Warning("Rejecting further transcription request");
            HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }

        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            log.Warning("Rejecting invalid transcription request");
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        log.Information("Accepting transcription request");
        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        alreadyConnected = true;
        frontendCommunicationService.ResetAbortedTracker();

        string format = await receiveFormatInformation(webSocket);

        bool validFormat = await validateAndSetFormat(format, webSocket);
        if (!validFormat) return;
        StartRecognitionMessage_TranscriptionConfig? transcriptionConfig = configurationService.GetCustomDictionary();
        CancellationTokenSource ctSource = new CancellationTokenSource();

        bool connectionSuccessful = await connectToSpeechmatics(webSocket);
        if (!connectionSuccessful) return;

        Task subtitleExportTask = subtitleExporterService.Start(webSocket, ctSource); // write at end of pipeline
        Task subtitleReceiveTask = await setupSpeechmaticsState(ctSource, transcriptionConfig);
        Task avReceiveTask = avReceiverService.Start(webSocket, ctSource); // read at start of pipeline

        await avReceiveTask; // no more audio to send

        // wait for all sent audio chunks to be received & confirmed by Speechmatics
        while (speechmaticsSendService.SequenceNumber > speechmaticsReceiveService.SequenceNumber &&
               !ctSource.Token.IsCancellationRequested)
        {
            log.Debug($"Waiting for receiving side's sequence number ({speechmaticsReceiveService.SequenceNumber}) "
                      + $"to match sending side's sequence number ({speechmaticsSendService.SequenceNumber})");
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        subtitleExporterService.RequestShutdown();
        await sendEndOfStreamMessage(ctSource);
        await subtitleReceiveTask; // no more subtitles to receive
        await speechmaticsConnectionService.Disconnect(!ctSource.IsCancellationRequested, makeConnectionTimeoutToken());
        await subtitleExportTask; // no more subtitles to export
        await closeConnectionWithClient(webSocket, ctSource);

        log.Information("Connection with client closed successfully");
        speechBubbleListService.Clear();
        alreadyConnected = false;
    }

    /// <summary>
    /// Task for receiving the format specification from the client.
    /// </summary>
    /// <param name="webSocket">WebSocket used for the connection to the client</param>
    /// <returns>String containing the selected format, empty String if no format could be obtained</returns>
    private async Task<string> receiveFormatInformation(WebSocket webSocket)
    {
        try
        {
            return await receiveFormatSpecification(webSocket);
        }
        catch (Exception e)
        {
            log.Error($"Failed to receive format specification from client: {e.Message}");
            log.Debug(e.ToString());
            return "";
        }
    }

    /// <summary>
    /// Validates the received Format and sets the Format in the SubtitleExporterService.
    /// </summary>
    /// <param name="format">The format requested by the client</param>
    /// <param name="webSocket">WebSocket used for the connection to the client</param>
    /// <returns>True if format is valid and was set successfully, otherwise False</returns>
    private async Task<bool> validateAndSetFormat(string format, WebSocket webSocket)
    {
        try
        {
            subtitleExporterService.SelectFormat(format);
        }
        catch (ArgumentException e)
        {
            try
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.InternalServerError,
                    e.Message,
                    makeConnectionTimeoutToken());
                log.Warning($"Rejecting transcription request with invalid subtitle format {format}");
            }
            catch (Exception webSocketCloseException)
            {
                log.Error($"Communication with Client failed: {webSocketCloseException.Message}");
                log.Debug(webSocketCloseException.ToString());
            }

            alreadyConnected = false;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Establishes a connection to Speechmatics.
    /// </summary>
    /// <param name="webSocket">WebSocket used for the connection to the client</param>
    /// <returns>True if connection was established successfully, otherwise False</returns>
    private async Task<bool> connectToSpeechmatics(WebSocket webSocket)
    {
        bool speechmaticsConnected = await speechmaticsConnectionService.Connect(makeConnectionTimeoutToken());
        if (!speechmaticsConnected)
        {
            log.Error("Failed to connect to Speechmatics");
            await speechmaticsConnectionService.Disconnect(false, makeConnectionTimeoutToken()); // cleanup & reset

            try
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.InternalServerError,
                    "Failed to connect to Speechmatics",
                    makeConnectionTimeoutToken());
            }
            catch (Exception e)
            {
                log.Error($"Communication with Client failed: {e.Message}");
                log.Debug(e.ToString());
            }

            alreadyConnected = false;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sets up the Speechmatics state by sending a StartRecognitionMessage and starting the receive loop.
    /// </summary>
    /// <param name="ctSource">Cancellation Token source used for cancelling the Task</param>
    /// <param name="transcriptionConfig">The obtained transcription configuration</param>
    /// <returns>The started subtitleReceiveTask</returns>
    private async Task<Task> setupSpeechmaticsState(CancellationTokenSource ctSource, StartRecognitionMessage_TranscriptionConfig? transcriptionConfig)
    {
        speechmaticsSendService.ResetSequenceNumber();
        Task subtitleReceiveTask = speechmaticsReceiveService.ReceiveLoop(ctSource);

        try
        {
            await speechmaticsSendService.SendJsonMessage<StartRecognitionMessage>(
                new StartRecognitionMessage(speechmaticsConnectionService.AudioFormat, transcriptionConfig));
        }
        catch (Exception e)
        {
            log.Error($"Failed to send start recognition message to Speechmatics: {e.Message}");
            log.Debug(e.ToString());
            ctSource.Cancel();
        }

        return subtitleReceiveTask;
    }

    /// <summary>
    /// Sends a EndOfStreamMessage to Speechmatics
    /// </summary>
    /// <param name="ctSource">Cancellation Token source used for cancelling the Task</param>
    private async Task sendEndOfStreamMessage(CancellationTokenSource ctSource)
    {
        try
        {
            await speechmaticsSendService.SendJsonMessage<EndOfStreamMessage>(
                new EndOfStreamMessage(speechmaticsSendService.SequenceNumber));
        }
        catch (Exception e)
        {
            log.Error($"Failed to send end of stream message to Speechmatics: {e.Message}");
            log.Debug(e.ToString());
            ctSource.Cancel();
        }
    }

    /// <summary>
    /// Sends a close request to the client and closes the connection.
    /// </summary>
    /// <param name="webSocket">WebSocket used for the connection to the client</param>
    /// <param name="ctSource">Cancellation Token source used for cancelling the Task</param>
    private async Task closeConnectionWithClient(WebSocket webSocket, CancellationTokenSource ctSource)
    {
        try
        {
            log.Information("Closing connection with client");
            await webSocket.CloseAsync(
                !ctSource.IsCancellationRequested
                    ? WebSocketCloseStatus.NormalClosure
                    : WebSocketCloseStatus.InternalServerError,
                "",
                makeConnectionTimeoutToken());
        }
        catch (Exception e)
        {
            log.Error("Failed to properly close communication with client");
            log.Error(e.Message);
            ctSource.Cancel();
        }
    }

    /// <summary>
    /// Receives a JSON formatted string response from a WebSocket.
    /// </summary>
    /// <param name="webSocket">The WebSocket from which to receive the response.</param>
    /// <returns>A string of the requested format.</returns>
    private async Task<string> receiveFormatSpecification(WebSocket webSocket)
    {
        byte[] chunkBuffer = new byte[RECEIVE_BUFFER_SIZE];
        List<byte> messageChunks = new List<byte>();
        bool completed = false;

        do
        {
            log.Debug("Listening for format data from Client");
            WebSocketReceiveResult response = await webSocket.ReceiveAsync(
                buffer: chunkBuffer,
                cancellationToken: makeConnectionTimeoutToken());
            log.Debug("Received format data from Client");

            // FIXME AddRange'ing a Span directly for better performance is a .NET 8 feature
            byte[] bufferToAdd = chunkBuffer;
            if (response.Count != chunkBuffer.Length)
            {
                bufferToAdd = new byte[response.Count];
                Array.Copy(chunkBuffer, bufferToAdd, response.Count);
            }

            messageChunks.AddRange(bufferToAdd);
            completed = response.EndOfMessage;
        }
        while (!completed);

        string completeMessage = Encoding.UTF8.GetString(messageChunks.ToArray());

        log.Debug($"Received format: {completeMessage}");
        return completeMessage;
    }
}
