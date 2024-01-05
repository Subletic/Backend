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
/// and returns the transcribed, corrected, and converted subtitles.
/// </summary>
[ApiController]
public class ClientExchangeController : ControllerBase
{
    private const int RECEIVE_BUFFER_SIZE = 8;
    private readonly IAvReceiverService avReceiverService;
    private readonly ISpeechmaticsConnectionService speechmaticsConnectionService;
    private readonly ISpeechmaticsReceiveService speechmaticsReceiveService;
    private readonly ISpeechmaticsSendService speechmaticsSendService;
    private readonly ISubtitleExporterService subtitleExporterService;
<<<<<<< Backend/Controllers/ClientExchangeController.cs
    private readonly ILogger log;
=======
    private readonly Serilog.ILogger log;
>>>>>>> Backend/Controllers/ClientExchangeController.cs

    /// <summary>
    /// Initializes the controller with necessary services via Dependency Injection.
    /// </summary>
<<<<<<< Backend/Controllers/ClientExchangeController.cs
    /// <param name="log">Logger for logging events and errors.</param>
    /// <param name="subtitleExporterService">Service for exporting subtitles.</param>
    /// <param name="avReceiverService">Service for receiving audio/video data.</param>
    public ClientExchangeController(ILogger log, ISubtitleExporterService subtitleExporterService, IAvReceiverService avReceiverService)
=======
    /// <param name="avReceiverService">The av receiver service.</param>
    /// <param name="speechmaticsConnectionService">The speechmatics connection management service.</param>
    /// <param name="speechmaticsReceiveService">The speechmatics message receive service.</param>
    /// <param name="speechmaticsSendService">The speechmatics message send service.</param>
    /// <param name="subtitleExporterService">The subtitle exporter service.</param>
    /// <param name="log">The logger.</param>
    public ClientExchangeController(
        IAvReceiverService avReceiverService,
        ISpeechmaticsConnectionService speechmaticsConnectionService,
        ISpeechmaticsReceiveService speechmaticsReceiveService,
        ISpeechmaticsSendService speechmaticsSendService,
        ISubtitleExporterService subtitleExporterService,
        Serilog.ILogger log)
>>>>>>> Backend/Controllers/ClientExchangeController.cs
    {
        this.log = log;
        this.avReceiverService = avReceiverService;
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.speechmaticsReceiveService = speechmaticsReceiveService;
        this.speechmaticsSendService = speechmaticsSendService;
        this.subtitleExporterService = subtitleExporterService;
        this.log = log;
    }

    /// <summary>
    /// Asynchronous method to handle WebSocket transcription requests.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    [Route("/transcribe")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
<<<<<<< Backend/Controllers/ClientExchangeController.cs
            log.Information("Rejecting invalid transcription request");
=======
            log.Warning("Rejecting invalid transcription request");
>>>>>>> Backend/Controllers/ClientExchangeController.cs
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        log.Information("Accepting transcription request");
        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

<<<<<<< Backend/Controllers/ClientExchangeController.cs
        try
        {
            // Receiving format information from the client.
            string formats = await receiveFormatSpecification(webSocket);

            // Validating and selecting the subtitle format.
            if (!isValidFormat(formats))
            {
                throw new ArgumentException("Unsupported subtitle format");
            }

            subtitleExporterService.SelectFormat(formats);
            CancellationTokenSource ctSource = new CancellationTokenSource();
            Task subtitleExportTask = subtitleExporterService.Start(webSocket, ctSource);
            Task avReceiveTask = avReceiverService.Start(webSocket, ctSource);

            await avReceiveTask;
            await subtitleExportTask;
        }
        catch (ArgumentException ex)
        {
            log.Error($"Error: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            log.Information("Cancellation handled");
        }
        finally
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }

    /// <summary>
    /// Receives a JSON-formatted string response via WebSocket.
    /// </summary>
    /// <param name="webSocket">WebSocket connection to receive the data from.</param>
    /// <returns>Complete JSON string received from the WebSocket.</returns>
    private async Task<string> receiveFormatSpecification(WebSocket webSocket)
    {
        byte[] chunkBuffer = new byte[RECEIVE_BUFFER_SIZE];
        var messageChunks = new List<byte>(RECEIVE_BUFFER_SIZE);
        bool completed = false;

        do
        {
            log.Debug("Listening for data from client");
            WebSocketReceiveResult response = await webSocket.ReceiveAsync(chunkBuffer, CancellationToken.None);
            log.Debug("Received data from client");

            byte[] bufferToAdd = response.Count != chunkBuffer.Length ? chunkBuffer[..response.Count] : chunkBuffer;
            messageChunks.AddRange(bufferToAdd);
            completed = response.EndOfMessage;
        }
        while (!completed);

        string completeMessage = Encoding.UTF8.GetString(messageChunks.ToArray());
        log.Debug($"Received message: {completeMessage}");
        return completeMessage;
    }

    /// <summary>
    /// Checks if the provided format string is a valid subtitle format.
    /// </summary>
    /// <param name="format">Subtitle format string to validate.</param>
    /// <returns>True if the format is valid; otherwise, false.</returns>
    private static bool isValidFormat(string format)
    {
        return format.Equals("webvtt", StringComparison.OrdinalIgnoreCase) ||
               format.Equals("srt", StringComparison.OrdinalIgnoreCase);
=======
        CancellationTokenSource ctSource = new CancellationTokenSource();

        // connect to Speechmatics
        bool speechmaticsConnected = await speechmaticsConnectionService.Connect(ctSource.Token);
        if (!speechmaticsConnected)
        {
            log.Error("Failed to connect to Speechmatics");
            await speechmaticsConnectionService.Disconnect(false, ctSource.Token); // cleanup & reset
            await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Failed to connect to Speechmatics", ctSource.Token);
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        Task subtitleExportTask = subtitleExporterService.Start(webSocket, ctSource); // write at end of pipeline

        // setup speechmatics state
        Task subtitleReceiveTask = speechmaticsReceiveService.ReceiveLoop();
        await speechmaticsSendService.SendJsonMessage<StartRecognitionMessage>(
            new StartRecognitionMessage(speechmaticsConnectionService.AudioFormat));

        Task<bool> avReceiveTask = avReceiverService.Start(webSocket, ctSource); // read at start of pipeline

        bool connectionAlive = await avReceiveTask; // no more audio to send

        if (connectionAlive)
        {
            // wait for all sent audio chunks to be received & confirmed by Speechmatics
            while (speechmaticsSendService.SequenceNumber > speechmaticsReceiveService.SequenceNumber)
            {
                log.Debug($"Waiting for receiving side's sequence number ({speechmaticsReceiveService.SequenceNumber}) to match sending side's sequence number ({speechmaticsSendService.SequenceNumber})");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            await speechmaticsSendService.SendJsonMessage<EndOfStreamMessage>(
                new EndOfStreamMessage(speechmaticsSendService.SequenceNumber));

            await subtitleReceiveTask; // no more subtitles to receive
            await speechmaticsConnectionService.Disconnect(connectionAlive, ctSource.Token);

            await subtitleExportTask; // no more subtitles to export
            await webSocket.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
        }
        else
        {
            // don't bother with more messages to Speechmatics, just end it
            await speechmaticsConnectionService.Disconnect(connectionAlive, ctSource.Token);

            try
            {
                await subtitleReceiveTask; // likely to throw due to broken connection, but need to reap the task
            }
            catch (Exception)
            {
                // ignore
            }

            try
            {
                await subtitleExportTask; // likely long-failed if connection is broken, but need to reap the task
            }
            catch (Exception)
            {
                // ignore
            }
        }
>>>>>>> Backend/Controllers/ClientExchangeController.cs
    }
}
