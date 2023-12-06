namespace Backend.Services;

using System.Net.WebSockets;
using System.Threading;
using Backend.Data;

/// <summary>
/// Interface for a service that exports subtitles.
/// </summary>
public interface ISubtitleExporterService
{
    /// <summary>
    /// Starts the subtitle export process.
    /// </summary>
    /// <param name="webSocket">The WebSocket to use for communication.</param>
    /// <param name="ctSource">The CancellationTokenSource to use for cancellation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task Start(WebSocket webSocket, CancellationTokenSource ctSource);

    /// <summary>
    /// Exports a speech bubble as a subtitle.
    /// </summary>
    /// <param name="speechBubble">The SpeechBubble to export.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task ExportSubtitle(SpeechBubble speechBubble);
}
