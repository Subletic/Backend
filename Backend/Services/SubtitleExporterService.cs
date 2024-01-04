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
    private Pipe subtitlePipe;

    /// <summary>
    /// The converter for translating SpeechBubbles into a preferred subtitle format
    /// </summary>
    private IWebVttConverter webVttConverter;

    /// <summary>
    /// The converter for translating SpeechBubbles into a preferred subtitle format
    /// </summary>
    private ISrtConverter srtConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleExporterService"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is used to handle dependency injection.
    /// </remarks>
    public SubtitleExporterService()
    {
        subtitlePipe = new Pipe();
        webVttConverter = new WebVttConverter(subtitlePipe.Writer.AsStream(leaveOpen: true));
        srtConverter = new SrtConverter(subtitlePipe.Writer.AsStream(leaveOpen: true));
    }

    /// <summary>
    /// Exports a speech bubble in the specified subtitle format.
    /// </summary>
    /// <param name="speechBubble">The speech bubble to export.</param>
    /// <param name="format">The subtitle format ("webvtt" or "srt").</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SelectFormat(SpeechBubble speechBubble, string format)
    {
        switch (format.ToLower())
        {
            case "webvtt":
                webVttConverter.ConvertSpeechBubbleToWebVtt(speechBubble);
                break;
            case "srt":
                srtConverter.ConvertSpeechBubblesToSrt(new List<SpeechBubble> { speechBubble });
                break;
            default:
                throw new ArgumentException("Unsupported subtitle format");
        }

        return Task.CompletedTask;
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

        Console.WriteLine("Start sending subtitles over WebSocket");

        try
        {
            while (true)
            {
                Console.WriteLine("Trying to read subtitles");
                int readCount = 0;
                try
                {
                    // "block" here until at least 1 byte can be read
                    readCount = await subtitleReaderStream.ReadAtLeastAsync(buffer, 1, true, ctSource.Token);
                }
                catch (EndOfStreamException)
                {
                    Console.WriteLine("End of stream reached");
                    break;
                }

                Console.WriteLine("Have subtitles ready to send");

                await webSocket.SendAsync(
                    new ReadOnlyMemory<byte>(buffer, 0, readCount),
                    WebSocketMessageType.Text,
                    false,
                    ctSource.Token);
                Console.WriteLine("Subtitles sent");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Subtitle export has been cancelled");
        }

        Console.WriteLine("Done sending subtitles over WebSocket");
    }

    /// <summary>
    /// Represents an asynchronous operation that can return a value.
    /// </summary>
    /// <param name="speechBubble">The speech bubble to export.</param
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ExportSubtitle(SpeechBubble speechBubble)
    {
        string format = determineFormat(speechBubble);
        SelectFormat(speechBubble, format);
        return Task.CompletedTask;
    }

    private string determineFormat(SpeechBubble speechBubble)
    {
        // Implementieren Sie hier Ihre Logik zur Bestimmung des Formats
        return "webvtt";
    }
}
