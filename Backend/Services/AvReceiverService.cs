namespace Backend.Services;

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Services;

/// <summary>
/// A service that fetches new A/V data over a WebSocket and kicks off its transcription via AvProcessingService.
/// </summary>
public class AvReceiverService : IAvReceiverService
{
    /// <summary>
    /// Maximum amount of data to read from the WebSocket at once, in bytes
    /// </summary>
    private const int MAXIMUM_READ_SIZE = 4 * 1024;

    /// <summary>
    /// Dependency Injection for AvProcessingService to push fetched data into
    /// </summary>
    private readonly IAvProcessingService avProcessingService;

    /// <summary>
    /// Dependency Injection for a logger
    /// </summary>
    private readonly Serilog.ILogger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvReceiverService"/> class.
    /// </summary>
    /// <param name="avProcessingService">The AvProcessingService to push fetched data into</param>
    /// <param name="log">The logger</param>
    public AvReceiverService(IAvProcessingService avProcessingService, Serilog.ILogger log)
    {
        this.avProcessingService = avProcessingService;
        this.log = log;
    }

    /// <summary>
    /// Starts the front part of the transcription pipeline (read A/V over WebSocket, push into AvProcessingService)
    /// </summary>
    /// <param name="webSocket">The WebSocket to read A/V data from</param>
    /// <param name="ctSource">The CancellationTokenSource to cancel the operation</param>
    /// <returns> A Task representing the asynchronous operation. </returns>
    public async Task<bool> Start(WebSocket webSocket, CancellationTokenSource ctSource)
    {
        Pipe avPipe = new Pipe();
        Stream avWriter = avPipe.Writer.AsStream(leaveOpen: true);
        WebSocketReceiveResult avResult;
        byte[] readBuffer = new byte[MAXIMUM_READ_SIZE];

        log.Debug("Start reading AV data from client");

        bool connectionAlive = true;
        Task<bool> processingTask = avProcessingService.PushProcessedAudio(avPipe.Reader.AsStream(leaveOpen: true));

        try
        {
            do
            {
                // too much
                // log.Debug("Waiting for AV data to arrive");
                avResult = await webSocket.ReceiveAsync(readBuffer, ctSource.Token);

                if (avResult.MessageType == WebSocketMessageType.Close)
                {
                    log.Information("Received WebSocket close request from client");
                    break;
                }

                // too much
                // log.Debug($"Pushing {avResult.Count} bytes into AV processing");
                await avWriter.WriteAsync(new ReadOnlyMemory<byte>(readBuffer, 0, avResult.Count), ctSource.Token);
            }
            while (avResult.MessageType != WebSocketMessageType.Close);
            log.Debug("Done reading AV data");
        }
        catch (WebSocketException e)
        {
            log.Error($"WebSocket to client has an error: {e.Message}");
            connectionAlive = false;
        }

        log.Debug("Closing pipe to AV processing");
        await avPipe.Writer.CompleteAsync();
        log.Debug("Waiting for AV processing to finish");
        bool processingSuccess = await processingTask;
        log.Debug("Processing " + (processingSuccess ? "success" : "failure"));

        return connectionAlive;
    }
}
