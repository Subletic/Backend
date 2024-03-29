namespace Backend.ClientCommunication;

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

    /// <summary>
    /// Selects the format for subtitle export.
    /// </summary>
    /// <param name="format">The format to use for exporting subtitles.</param>
    public void SelectFormat(string format);

    /// <summary> Called from BufferTimeMonitor to let the ExporterService know if there are remaining SpeechBubbles in the queue.
    /// Used for shutting down after all SpeechBubbles have been exported.
    /// </summary>
    /// <param name="containsItems">true if queue contains SpeechBubbles</param>
    public void SetQueueContainsItems(bool containsItems);

    /// <summary>
    /// Called from ClientExchangeController to tell the ExporterService that it is ready for shutdown.
    /// </summary>
    public void RequestShutdown();
}
