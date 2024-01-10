namespace Backend.Services;

using Backend.Data;

/// <summary>
/// Interface defining services for frontend communication, including audio streaming and speech bubble management.
/// </summary>
public interface IFrontendCommunicationService
{
    /// <summary>
    /// Enqueues an audio buffer into the queue.
    /// </summary>
    /// <param name="item">The audio buffer to be enqueued.</param>
    void Enqueue(short[] item);

    /// <summary>
    /// Attempts to dequeue an audio buffer element from the queue.
    /// </summary>
    /// <param name="item">The dequeued audio buffer element, if available.</param>
    /// <returns>
    /// <c>true</c> if an element was successfully dequeued; otherwise, <c>false</c>.
    /// </returns>
    bool TryDequeue(out short[]? item);

    /// <summary>
    /// Requests the deletion of a speech bubble, identified by its ID, from the frontend.
    /// </summary>
    /// <param name="id">The unique identifier of the speech bubble to be deleted.</param>
    /// <returns>DeleteSpeechBubble.</returns>
    Task DeleteSpeechBubble(long id);

    /// <summary>
    /// Publishes a new speech bubble to the frontend.
    /// This method is used to send data related to speech bubbles to the frontend for display or further processing.
    /// </summary>
    /// <param name="speechBubble">The speech bubble object to be sent to the frontend.</param>
    /// <returns>PublishSpeechBubble.</returns>
    Task PublishSpeechBubble(SpeechBubble speechBubble);
}
