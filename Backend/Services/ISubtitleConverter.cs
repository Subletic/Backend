namespace Backend.Services;

using Backend.Data;
using Backend.Services;

/// <summary>
/// Interface for converting speech bubbles to subtitles.
/// </summary>
public interface ISubtitleConverter
{
    /// <summary>
    /// Converts a speech bubble to a subtitle.
    /// </summary>
    /// <param name="speechBubble">The speech bubble to convert.</param>
    public void ConvertSpeechBubble(SpeechBubble speechBubble);
}
