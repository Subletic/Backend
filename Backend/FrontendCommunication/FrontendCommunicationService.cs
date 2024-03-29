namespace Backend.FrontendCommunication;

using System.Collections.Concurrent;
using Backend.Data;
using Backend.FrontendCommunication;
using Microsoft.AspNetCore.SignalR;
using ILogger = Serilog.ILogger;

/// <summary>
/// Provides services to handle frontend requests, including audio streaming and speech bubble management.
/// </summary>
public class FrontendCommunicationService : IFrontendCommunicationService
{
    /// <summary>
    /// The length of the audio buffer used in audio streaming.
    /// This defines the expected size of each audio buffer element in the queue.
    /// </summary>
    private const int AUDIO_FRQUENCY = 48000;

    /// <summary>
    /// Logger instance for logging events and errors.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// Context for interacting with SignalR hubs, used to communicate with the frontend.
    /// </summary>
    private readonly IHubContext<FrontendCommunicationHub> hubContext;

    /// <summary>
    /// A thread-safe queue for storing audio buffers to be processed.
    /// </summary>
    private ConcurrentQueue<short[]> audioQueue = new ConcurrentQueue<short[]>();

    /// <summary>
    /// Tracker to make sure abortCorrection frontend endpoint is only called once per processing pipeline.
    /// </summary>
    private bool frontendCorrectionAborted = true;

    /// <summary>
    /// Initializes a new instance of the FrontendProviderService class.
    /// </summary>
    /// <param name="logger">Logger for logging events and errors.</param>
    /// <param name="hubContext">Context for interacting with SignalR hubs.</param>
    public FrontendCommunicationService(ILogger logger, IHubContext<FrontendCommunicationHub> hubContext)
    {
        this.logger = logger;
        this.hubContext = hubContext;
    }

    /// <summary>
    /// Checks and enqueues the buffer.
    /// </summary>
    /// <exception cref="ArgumentException">Buffer has wrong amount of samples</exception>
    /// <param name="item">Buffer to enqueue</param>
    public void Enqueue(short[] item)
    {
        if (frontendCorrectionAborted)
        {
            logger.Debug("Frontend correction process has been aborted, refusing to enqueue new audio data");
            return;
        }

        if (item.Length != AUDIO_FRQUENCY) throw new ArgumentException($"Enqueued buffer ({item.Length} samples) doesn't have correct element count ({AUDIO_FRQUENCY} samples)");
        audioQueue.Enqueue(item);
    }

    /// <summary>
    /// Versucht, ein Audio-Buffer-Element aus der Warteschlange zu entnehmen.
    /// </summary>
    /// <param name="item">Das entnommene Audio-Buffer-Element, falls vorhanden.</param>
    /// <returns>
    /// <c>true</c>, wenn ein Element erfolgreich entnommen wurde; andernfalls <c>false</c>.
    /// </returns>
    public bool TryDequeue(out short[]? item)
    {
        return audioQueue.TryDequeue(out item);
    }

    /// <summary>
    /// Deletes a speech bubble identified by its ID and informs the frontend about the deletion.
    /// </summary>
    /// <param name="id">The ID of the speech bubble to be deleted.</param>
    /// <returns>DeleteSpeechBubble.</returns>
    public async Task DeleteSpeechBubble(long id)
    {
        try
        {
            await hubContext.Clients.All.SendAsync("deleteBubble", id);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to transmit id of deleted Speechbubble to Frontend: {ex.Message}");
        }
    }

    /// <summary>
    /// Publishes a new speech bubble to the frontend.
    /// </summary>
    /// <param name="speechBubble">The speech bubble to be sent to the frontend.</param>
    /// <returns>PublishSpeechBubble.</returns>
    public async Task PublishSpeechBubble(SpeechBubble speechBubble)
    {
        if (frontendCorrectionAborted)
        {
            logger.Debug("Frontend correction process has been aborted, refusing to publish new speech bubble");
            return;
        }

        try
        {
            var listToSend = new List<SpeechBubble>() { speechBubble };
            await hubContext.Clients.All.SendAsync("newBubble", listToSend);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to transmit to Frontend: {ex.Message}");
        }
    }

    /// <summary>
    /// Reset the tracker that ensures frontend processing is only aborted once per processing.
    /// </summary>
    public void ResetAbortedTracker()
    {
        frontendCorrectionAborted = false;
    }

    /// <summary>
    /// Inform the Frontend that an error occurred and it should stop the subtitle correction process.
    /// </summary>
    /// <param name="errorMessage">The error message to be displayed in the frontend. Should be mostly German, because it's user-facing.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task AbortCorrection(string errorMessage)
    {
        if (frontendCorrectionAborted)
        {
            logger.Error($"Attempted to abort frontend processing more than once, cancellation appears to not be caught properly somewhere!");
            return;
        }

        try
        {
            await hubContext.Clients.All.SendAsync("abortCorrection", errorMessage);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to inform the Frontend that it should abort its processing: {ex.Message}");
        }

        frontendCorrectionAborted = true;
    }
}
