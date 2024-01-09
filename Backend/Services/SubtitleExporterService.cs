namespace Backend.Services;

using System.IO.Pipelines;
using System.Net.WebSockets;
using Backend.Data;
using Serilog;
using ILogger = Serilog.ILogger;

/// <summary>
/// Service responsible for exporting finished subtitles back over the WebSocket connection.
/// </summary>
public class SubtitleExporterService : ISubtitleExporterService
{
    /// <summary>
    /// Maximum amount of data to read from the converted subtitle at once, in bytes
    /// </summary>
    private const int MAXIMUM_READ_SIZE = 4096;

    /// <summary>
    /// Dependency Injection for the application configuration
    /// </summary>
    private readonly IConfiguration configuration;

    /// <summary>
    /// Dependency Injection for the logger
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Pipe for reading converted subtitles from the converter
    /// </summary>
    private Pipe subtitlePipe;

    /// <summary>
    /// The converter for translating SpeechBubbles into a preferred subtitle format
    /// </summary>
    private ISubtitleConverter? subtitleConverter;

    /// <summary>
    /// True if SpeechBubbleListService still contains items deemed for export
    /// </summary>
    private bool queueContainsItems;

    /// <summary>
    /// True if Client requested Shutdown after transcription finished
    /// </summary>
    private bool shutdownRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleExporterService"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is used to handle dependency injection.
    /// </remarks>
    /// <param name="log"> DI Serilog reference </param>
    public SubtitleExporterService(IConfiguration configuration, ILogger log)
    {
        this.configuration = configuration;
        this.log = log;
        subtitlePipe = new Pipe();
    }

    /// <summary>
    /// Exports a speech bubble in the specified subtitle format.
    /// </summary>
    /// <param name="format">The subtitle format ("webvtt" or "srt").</param>
    public void SelectFormat(string format)
    {
        this.subtitlePipe = new Pipe();

        string formatLower = format.ToLower();
        switch (formatLower)
        {
            case "vtt":
                subtitleConverter = new WebVttConverter(subtitlePipe.Writer.AsStream(leaveOpen: true));
                return;
            case "srt":
                subtitleConverter = new SrtConverter(subtitlePipe.Writer.AsStream(leaveOpen: true));
                return;
            default:
                throw new ArgumentException($"Unsupported subtitle format {formatLower}, must be one of: vtt, srt");
        }
    }

    /// <summary>
    /// Starts up the sending side of the processing pipeline.
    /// Awaits subtitles to be pushed into <c>ExportSubtitle</c>, receives converted subtitles from
    /// chosen subtitle converter and pushes them back through the WebSocket connection.
    /// </summary>
    /// <param name="webSocket">The WebSocket connection to send subtitles over</param>
    /// <param name="ctSource">The cancellation token source to cancel the export</param>
    /// <returns>Successful Task Completion</returns>
    public async Task Start(WebSocket webSocket, CancellationTokenSource ctSource)
    {
        Stream subtitleReaderStream = subtitlePipe.Reader.AsStream(leaveOpen: false);
        byte[] buffer = new byte[MAXIMUM_READ_SIZE];

        log.Information("Start sending subtitles over WebSocket");

        queueContainsItems = false;
        shutdownRequested = false;

        try
        {
            while (true)
            {
                log.Debug("Trying to read subtitles");
                log.Debug("Queue contains items: {QueueContainsItems}", queueContainsItems);
                int readCount = 0;
                try
                {
                    if (shutdownRequested && !queueContainsItems)
                    {
                        log.Debug("Shutting down export");
                        subtitleReaderStream.Close();
                        break;
                    }

                    // "block" here until at least 1 byte can be read
                    readCount = await subtitleReaderStream.ReadAtLeastAsync(buffer, 1, true, ctSource.Token);
                }
                catch (EndOfStreamException)
                {
                    log.Information("End of stream reached");
                    break;
                }

                log.Debug("Have subtitles ready to send");

                CancellationToken timeout = new CancellationTokenSource(
                    (int)TimeSpan
                        .FromSeconds(configuration.GetValue<double>("ClientCommunicationSettings:TIMEOUT_IN_SECONDS"))
                        .TotalMilliseconds).Token;

                try
                {
                    await webSocket.SendAsync(
                        new ReadOnlyMemory<byte>(buffer, 0, readCount),
                        WebSocketMessageType.Text,
                        false,
                        timeout);
                }
                catch (OperationCanceledException e)
                {
                    log.Error("Timed out waiting for client to receive subtitles");
                    throw e;
                }

                log.Debug("Subtitles sent");
                ctSource.Token.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            log.Information("Subtitle export has been cancelled");
        }
        catch (Exception e)
        {
            log.Error("Exception occured while trying to export subtitles: {Exception}", e);
            ctSource.Cancel();
        }

        log.Information("Done sending subtitles over WebSocket");
    }

    /// <summary>
    /// Called from BufferTimeMonitor to let the ExporterService know if there are remaining SpeechBubbles in the queue.
    /// Used for shutting down after all SpeechBubbles have been exported.
    /// </summary>
    /// <param name="containsItems">true if queue contains SpeechBubbles</param>
    public void SetQueueContainsItems(bool containsItems)
    {
        queueContainsItems = containsItems;
    }

    /// <summary>
    /// Called from ClientExchangeController to tell the ExporterService that it is ready for shutdown.
    /// </summary>
    public void RequestShutdown()
    {
        log.Debug("Shutdown requested!");
        shutdownRequested = true;
    }

    /// <summary>
    /// Represents an asynchronous operation that can return a value.
    /// </summary>
    /// <param name="speechBubble">The speech bubble to export.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ExportSubtitle(SpeechBubble speechBubble)
    {
        if (subtitleConverter is null) throw new InvalidOperationException("Not Valid subtitle");

        subtitleConverter.ConvertSpeechBubble(speechBubble);
        return Task.CompletedTask;
    }
}
