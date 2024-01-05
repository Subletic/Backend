namespace Backend.Controllers;

using System.Net;
using System.Net.WebSockets;
using System.Text;
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
    private const int RECEIVE_BUFFER_SIZE = 8;
    private readonly IAvReceiverService avReceiverService;
    private readonly ISubtitleExporterService subtitleExporterService;
    private readonly Serilog.ILogger log;

    /// <summary>
    /// Constructor for ClientExchangeController.
    /// Gets instances of services via Dependency Injection.
    /// </summary>
    /// <param name="log">The logger</param>
    /// <param name="subtitleExporterService">The subtitle exporter service.</param>
    /// <param name="avReceiverService">The av receiver service.</param>
    public ClientExchangeController(Serilog.ILogger log, ISubtitleExporterService subtitleExporterService, IAvReceiverService avReceiverService)
    {
        this.log = log;
        this.avReceiverService = avReceiverService;
        this.subtitleExporterService = subtitleExporterService;
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
            log.Information("Rejecting invalid transcription request");
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        log.Information("Accepting transcription request");
        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        string formats = await receiveJsonResponse(webSocket);
        subtitleExporterService.SelectFormat(formats);
        CancellationTokenSource ctSource = new CancellationTokenSource();
        Task subtitleExportTask = subtitleExporterService.Start(webSocket, ctSource); // write at end of pipeline
        Task avReceiveTask = avReceiverService.Start(webSocket, ctSource); // read at start of pipeline

        await avReceiveTask;
        try
        {
            await subtitleExportTask;
        }
        catch (OperationCanceledException)
        {
            log.Information("Cancellation handled");
        }

        await webSocket.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
    }

    /// <summary>
    /// Receives a JSON formatted string response from a WebSocket.
    /// </summary>
    /// <param name="webSocket">The WebSocket from which to receive the response.</param>
    /// <returns>A string of the JSON response.</returns>
    private async Task<string> receiveJsonResponse(WebSocket webSocket)
    {
        byte[] chunkBuffer = new byte[RECEIVE_BUFFER_SIZE];
        List<byte> messageChunks = new List<byte>(RECEIVE_BUFFER_SIZE);
        bool completed = false;

        do
        {
            log.Debug("Listening from data from Speechmatics");
            WebSocketReceiveResult response = await webSocket.ReceiveAsync(
                buffer: chunkBuffer,
                cancellationToken: CancellationToken.None);
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
