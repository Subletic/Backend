namespace Backend.Controllers;

using System.Net;
using System.Net.WebSockets;
using System.Threading;

using Backend.Data;
using Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage;
using Backend.Services;

using Microsoft.AspNetCore.Mvc;

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
    private readonly IAvReceiverService avReceiverService;
    private readonly ISpeechmaticsConnectionService speechmaticsConnectionService;
    private readonly ISpeechmaticsReceiveService speechmaticsReceiveService;
    private readonly ISpeechmaticsSendService speechmaticsSendService;
    private readonly ISubtitleExporterService subtitleExporterService;
    private readonly Serilog.ILogger log;

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
        ISpeechmaticsConnectionService speechmaticsConnectionService,
        ISpeechmaticsReceiveService speechmaticsReceiveService,
        ISpeechmaticsSendService speechmaticsSendService,
        ISubtitleExporterService subtitleExporterService,
        Serilog.ILogger log)
    {
        this.avReceiverService = avReceiverService;
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.speechmaticsReceiveService = speechmaticsReceiveService;
        this.speechmaticsSendService = speechmaticsSendService;
        this.subtitleExporterService = subtitleExporterService;
        this.log = log;
    }

    /// <summary>
    /// Represents an asynchronous operation that can return a value.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Route("/transcribe")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            log.Warning("Rejecting invalid transcription request");
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        log.Information("Accepting transcription request");
        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

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
    }
}
