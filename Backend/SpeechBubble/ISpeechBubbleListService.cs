namespace Backend.SpeechBubble;

using Backend.Data;

/// <summary>
/// Interface for a service that manages a list of speech bubbles.
/// </summary>
public interface ISpeechBubbleListService
{
    /// <summary>
    /// Returns the list of speech bubbles.
    /// </summary>
    /// <returns>The list of speech bubbles.</returns>
    public LinkedList<SpeechBubble> GetSpeechBubbles();

    /// <summary>
    /// Adds a new speech bubble to the list.
    /// </summary>
    /// <param name="speechBubble">The speech bubble to add.</param>
    public void AddNewSpeechBubble(SpeechBubble speechBubble);

    /// <summary>
    /// Deletes the oldest speech bubble from the list.
    /// </summary>
    public void DeleteOldestSpeechBubble();

    /// <summary>
    /// Replaces a speech bubble in the list with a new one.
    /// </summary>
    /// <param name="speechBubble">The new speech bubble.</param>
    public void ReplaceSpeechBubble(SpeechBubble speechBubble);

    /// <summary>
    /// Clear the list.
    /// </summary>
    public void Clear();
}
