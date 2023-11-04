using Backend.Data;
using Backend.Services;

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Service responsible for exporting finished subtitles back over the WebSocket connection.
/// </summary>
public class SubtitleExporterService : ISubtitleExporterService
{
    private const int MINIMUM_READ_SIZE = 10;

    private Pipe subtitlePipe;

    private ISubtitleConverter subtitleConverter;

    /// <summary>
    /// Basic constructor.
    /// </summary>
    public SubtitleExporterService()
    {
        subtitlePipe = new Pipe();
        subtitleConverter = new WebVttConverter (subtitlePipe.Writer.AsStream (leaveOpen: true));
    }

    /// <summary>
    /// Starts up the sending side of the processing pipeline.
    /// Awaits subtitles to be pushed into <c>ExportSubtitle</c>, receives converted subtitles from
    /// chosen subtitle converter and pushes them back through the WebSocket connection.
    /// </summary>
    public async Task Start(WebSocket webSocket, CancellationTokenSource ctSource)
    {
        PipeReader subtitleReader = subtitlePipe.Reader;
        ReadResult subtitleResult;

        Console.WriteLine ("Start sending subtitles over WebSocket");

        do
        {
            subtitleResult = await subtitleReader.ReadAtLeastAsync (MINIMUM_READ_SIZE, ctSource.Token);
            if (subtitleResult.IsCanceled)
            {
                Console.WriteLine ("Cancellation has been triggered");
                break;
            }

            Console.WriteLine ("Have subtitles ready to send");

            await webSocket.SendAsync (BuffersExtensions.ToArray<byte> (subtitleResult.Buffer),
                WebSocketMessageType.Text,
                subtitleResult.IsCompleted,
                ctSource.Token);
        }
        while (!subtitleResult.IsCompleted);
    }

    /// <summary>
    /// Call to push a finished <c>SpeechBubble</c> through the chosen converter.
    /// </summary>
    public Task ExportSubtitle(SpeechBubble speechBubble)
    {
        var subtitleText = subtitleConverter.ConvertSpeechBubble(speechBubble);
        return Task.CompletedTask;
    }
}
