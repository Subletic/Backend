namespace Backend.Controllers;

using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Backend.Data;
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
    private readonly ISubtitleExporterService subtitleExporterService;
    private readonly ILogger log;

    /// <summary>
    /// Initializes the controller with necessary services via Dependency Injection.
    /// </summary>
    /// <param name="log">Logger for logging events and errors.</param>
    /// <param name="subtitleExporterService">Service for exporting subtitles.</param>
    /// <param name="avReceiverService">Service for receiving audio/video data.</param>
    public ClientExchangeController(ILogger log, ISubtitleExporterService subtitleExporterService, IAvReceiverService avReceiverService)
    {
        this.log = log;
        this.avReceiverService = avReceiverService;
        this.subtitleExporterService = subtitleExporterService;
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
            log.Information("Rejecting invalid transcription request");
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        log.Information("Accepting transcription request");
        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

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
    private bool isValidFormat(string format)
    {
        return format.Equals("webvtt", StringComparison.OrdinalIgnoreCase) ||
               format.Equals("srt", StringComparison.OrdinalIgnoreCase);
    }
}
