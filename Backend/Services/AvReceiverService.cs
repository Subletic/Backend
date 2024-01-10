namespace Backend.Services;

using System.IO.Pipelines;
using System.Net.WebSockets;
using ILogger = Serilog.ILogger;

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
    /// Dependency Injection for the application's configuration
    /// </summary>
    private readonly IConfiguration configuration;

    /// <summary>
    /// Dependency Injection for a logger
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvReceiverService"/> class.
    /// </summary>
    /// <param name="avProcessingService">The AvProcessingService to push fetched data into</param>
    /// <param name="log">The logger</param>
    public AvReceiverService(
        IAvProcessingService avProcessingService,
        IConfiguration configuration,
        ILogger log)
    {
        this.avProcessingService = avProcessingService;
        this.configuration = configuration;
        this.log = log;
    }

    /// <summary>
    /// Starts the front part of the transcription pipeline (read A/V over WebSocket, push into AvProcessingService)
    /// </summary>
    /// <param name="webSocket">The WebSocket to read A/V data from</param>
    /// <param name="ctSource">The CancellationTokenSource to cancel the operation</param>
    /// <returns> A Task representing the asynchronous operation. </returns>
    public async Task Start(WebSocket webSocket, CancellationTokenSource ctSource)
    {
        Pipe avPipe = new Pipe();
        Stream avWriter = avPipe.Writer.AsStream(leaveOpen: true);
        WebSocketReceiveResult avResult;
        byte[] readBuffer = new byte[MAXIMUM_READ_SIZE];

        log.Debug("Start reading AV data from client");

        Task<bool> processingTask = avProcessingService.PushProcessedAudio(avPipe.Reader.AsStream(leaveOpen: true), ctSource);

        try
        {
            do
            {
                CancellationTokenSource timeout = new CancellationTokenSource(
                    (int)TimeSpan.FromSeconds(configuration.GetValue<double>("ClientCommunicationSettings:TIMEOUT_IN_SECONDS"))
                        .TotalMilliseconds);

                // differenciate between timeout being hit and the shared token being cancelled
                try
                {
                    avResult = await webSocket.ReceiveAsync(readBuffer, timeout.Token);
                }
                catch (OperationCanceledException)
                {
                    log.Error("Timed out waiting for client to send AV data");
                    throw;
                }

                ctSource.Token.ThrowIfCancellationRequested();

                if (avResult.MessageType == WebSocketMessageType.Close)
                {
                    log.Information("Received WebSocket close request from client");
                    break;
                }

                await avWriter.WriteAsync(new ReadOnlyMemory<byte>(readBuffer, 0, avResult.Count), ctSource.Token);
            }
            while (avResult.MessageType != WebSocketMessageType.Close);
            log.Debug("Done reading AV data");
        }
        catch (OperationCanceledException)
        {
            log.Error("Reading AV data from client has been cancelled");
        }
        catch (Exception e)
        {
            log.Error($"WebSocket to client has an error: {e.Message}");
            log.Debug(e.ToString());
            ctSource.Cancel();
        }

        log.Debug("Closing pipe to AV processing");
        await avPipe.Writer.CompleteAsync();

        log.Debug("Waiting for AV processing to finish");
        bool processingSuccess = await processingTask;

        log.Debug("Processing " + (processingSuccess ? "success" : "failure"));
    }
}
