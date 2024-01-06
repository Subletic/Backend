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
    /// Pipe for reading converted subtitles from the converter
    /// </summary>
    private readonly Pipe subtitlePipe;

    private readonly ILogger log;

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
    public SubtitleExporterService(ILogger log)
    {
        this.log = log;
        subtitlePipe = new Pipe();
    }

    /// <summary>
    /// Exports a speech bubble in the specified subtitle format.
    /// </summary>
    /// <param name="format">The subtitle format ("webvtt" or "srt").</param>
    public void SelectFormat(string format)
    {
        switch (format.ToLower())
        {
            case "webvtt":
                subtitleConverter = new WebVttConverter(subtitlePipe.Writer.AsStream(leaveOpen: true));
                break;
            case "srt":
                subtitleConverter = new SrtConverter(subtitlePipe.Writer.AsStream(leaveOpen: true));
                break;
            default:
                throw new ArgumentException("Unsupported subtitle format");
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

        try
        {
            while (true)
            {
                Log.Debug("Trying to read subtitles");
                Log.Debug("Queue contains items: {QueueContainsItems}", queueContainsItems);
                int readCount = 0;
                try
                {
                    if (shutdownRequested && !queueContainsItems)
                    {
                        Log.Debug("Shutting down export");
                        subtitleReaderStream.Close();
                        break;
                    }

                    // "block" here until at least 1 byte can be read
                    readCount = await subtitleReaderStream.ReadAtLeastAsync(buffer, 1, true, ctSource.Token);
                }
                catch (EndOfStreamException)
                {
                    Log.Information("End of stream reached");
                    break;
                }

                Log.Debug("Have subtitles ready to send");

                await webSocket.SendAsync(
                    new ReadOnlyMemory<byte>(buffer, 0, readCount),
                    WebSocketMessageType.Text,
                    false,
                    ctSource.Token);
                Log.Debug("Subtitles sent");
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Subtitle export has been cancelled");
        }

        Log.Information("Done sending subtitles over WebSocket");
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
        Log.Debug("Shutdown requested!");
        shutdownRequested = true;
    }

    /// <summary>
    /// Represents an asynchronous operation that can return a value.
    /// </summary>
    /// <param name="speechBubble">The speech bubble to export.</param
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ExportSubtitle(SpeechBubble speechBubble)
    {
        if (subtitleConverter is null) throw new InvalidOperationException("Not Valid subtitle");

        subtitleConverter.ConvertSpeechBubble(speechBubble);
        return Task.CompletedTask;
    }
}
