namespace Backend.Controllers;

using System.Net;
using System.Net.WebSockets;
using System.Threading;
using Backend.Data;
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
    private readonly ISpeechmaticsExchangeService speechmaticsExchangeService;
    private readonly ISubtitleExporterService subtitleExporterService;
    private readonly Serilog.ILogger log;

    /// <summary>
    /// Constructor for ClientExchangeController.
    /// Gets instances of services via Dependency Injection.
    /// </summary>
    /// <param name="subtitleExporterService">The subtitle exporter service.</param>
    /// <param name="avReceiverService">The av receiver service.</param>
    public ClientExchangeController(
        IAvReceiverService avReceiverService,
        ISpeechmaticsExchangeService speechmaticsExchangeService,
        ISubtitleExporterService subtitleExporterService,
        Serilog.ILogger log)
    {
        this.avReceiverService = avReceiverService;
        this.speechmaticsExchangeService = speechmaticsExchangeService;
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
        bool speechmaticsConnected = await speechmaticsExchangeService.Connect(ctSource);
        if (!speechmaticsConnected)
        {
            log.Error("Failed to connect to Speechmatics");
            await speechmaticsExchangeService.Disconnect(ctSource); // cleanup & reset
            await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Failed to connect to Speechmatics", ctSource.Token);
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        Task subtitleExportTask = subtitleExporterService.Start(webSocket, ctSource); // write at end of pipeline
        Task avReceiveTask = avReceiverService.Start(webSocket, ctSource); // read at start of pipeline

        await avReceiveTask; // receiving new audio finishes first
        await speechmaticsExchangeService.Disconnect(ctSource); // then sending the audio through speechmatics for the transcription
        try
        {
            await subtitleExportTask; // lastly running transcription through the user & exporting them
        }
        catch (OperationCanceledException)
        {
            log.Information("Cancellation handled");
        }

        await webSocket.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
    }
}
