namespace Backend.Services;

using Backend.Data;

/// <summary>
/// Defines an interface for a service that handles new word tokens.
/// </summary>
public interface IWordProcessingService
{
    /// <summary>
    /// Handles a new word token.
    /// </summary>
    /// <param name="wordToken">The word token to handle.</param>
    public Task HandleNewWord(WordToken wordToken);

    /// <summary>
    /// Method to flush the WordToken buffer to a new SpeechBubble.
    /// </summary>
    public Task FlushBufferToNewSpeechBubble();
}
