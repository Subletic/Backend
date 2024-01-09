namespace Backend.Controllers;

using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

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
    private readonly Serilog.ILogger log;

    private static bool alreadyConnected = false;

    private TimeSpan clientTimeout;

    /// <summary>
    /// Constructor for ClientExchangeController.
    /// Gets instances of services via Dependency Injection.
    /// </summary>
    /// <param name="avReceiverService">The av receiver service.</param>
    /// <param name="speechmaticsConnectionService">The speechmatics connection management service.</param>
    /// <param name="speechmaticsReceiveService">The speechmatics message receive service.</param>
    /// <param name="speechmaticsSendService">The speechmatics message send service.</param>
    /// <param name="subtitleExporterService">The subtitle exporter service.</param>
    /// <param name="log">The logger.</param>
    public ClientExchangeController(
        IAvReceiverService avReceiverService,
        ISpeechBubbleListService speechBubbleListService,
        ISpeechmaticsConnectionService speechmaticsConnectionService,
        ISpeechmaticsReceiveService speechmaticsReceiveService,
        ISpeechmaticsSendService speechmaticsSendService,
        ISubtitleExporterService subtitleExporterService,
        IConfiguration configuration,
        Serilog.ILogger log)
    {
        this.avReceiverService = avReceiverService;
        this.speechBubbleListService = speechBubbleListService;
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.speechmaticsReceiveService = speechmaticsReceiveService;
        this.speechmaticsSendService = speechmaticsSendService;
        this.subtitleExporterService = subtitleExporterService;
        this.configuration = configuration;
        this.log = log;
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

        // Receiving format information from the client.
        string formats = "";
        try
        {
            formats = await receiveFormatSpecification(webSocket);
        }
        catch (Exception e)
        {
            log.Error($"Failed to receive format specification from client: {e.Message}");
            log.Debug(e.ToString());
        }

        // Validating and selecting the subtitle format.
        try
        {
            subtitleExporterService.SelectFormat(formats);
        }
        catch (ArgumentException e)
        {
            try
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.InternalServerError,
                    e.Message,
                    makeConnectionTimeoutToken());
                log.Warning($"Rejecting transcription request with invalid subtitle format {formats}");
            }
            catch (Exception webSocketCloseException)
            {
                log.Error($"Communication with Client failed: {webSocketCloseException.Message}");
                log.Debug(webSocketCloseException.ToString());
            }

            return;
        }

        CancellationTokenSource ctSource = new CancellationTokenSource();

        // connect to Speechmatics
        bool speechmaticsConnected = await speechmaticsConnectionService.Connect(makeConnectionTimeoutToken());
        if (!speechmaticsConnected)
        {
            log.Error("Failed to connect to Speechmatics");
            await speechmaticsConnectionService.Disconnect(false, ctSource.Token); // cleanup & reset

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

            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        Task subtitleExportTask = subtitleExporterService.Start(webSocket, ctSource); // write at end of pipeline

        // setup speechmatics state
        speechmaticsSendService.ResetSequenceNumber();
        Task subtitleReceiveTask = speechmaticsReceiveService.ReceiveLoop(ctSource);

        try
        {
            await speechmaticsSendService.SendJsonMessage<StartRecognitionMessage>(
                new StartRecognitionMessage(speechmaticsConnectionService.AudioFormat));
        }
        catch (Exception e)
        {
            log.Error($"Failed to send start recognition message to Speechmatics: {e.Message}");
            log.Debug(e.ToString());
            ctSource.Cancel();
        }

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

        if (!ctSource.IsCancellationRequested)
            await speechmaticsSendService.SendJsonMessage<EndOfStreamMessage>(new EndOfStreamMessage(speechmaticsSendService.SequenceNumber));

        if (!ctSource.IsCancellationRequested)
            await subtitleReceiveTask; // no more subtitles to receive

        if (!ctSource.IsCancellationRequested)
            await speechmaticsConnectionService.Disconnect(!ctSource.IsCancellationRequested, ctSource.Token);

        if (!ctSource.IsCancellationRequested)
            await subtitleExportTask; // no more subtitles to export

        if (!ctSource.IsCancellationRequested)
        {
            try
            {
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", makeConnectionTimeoutToken());
            }
            catch (Exception e)
            {
                log.Error("Failed to properly close communication with client");
                log.Error(e.Message);
                ctSource.Cancel();
            }
        }

        log.Information("Connection with client closed successfully");

        speechBubbleListService.Clear();
        alreadyConnected = false;
    }

    /// <summary>
    /// Receives a JSON formatted string response from a WebSocket.
    /// </summary>
    /// <param name="webSocket">The WebSocket from which to receive the response.</param>
    /// <returns>A string of the JSON response.</returns>
    private async Task<string> receiveFormatSpecification(WebSocket webSocket)
    {
        byte[] chunkBuffer = new byte[RECEIVE_BUFFER_SIZE];
        List<byte> messageChunks = new List<byte>(RECEIVE_BUFFER_SIZE);
        bool completed = false;

        do
        {
            log.Debug("Listening from data from Speechmatics");
            WebSocketReceiveResult response = await webSocket.ReceiveAsync(
                buffer: chunkBuffer,
                cancellationToken: makeConnectionTimeoutToken());
            log.Debug("Received data from Speechmatics");

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

        log.Debug($"Received message: {completeMessage}");
        return completeMessage;
    }
}
