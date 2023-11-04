using Backend.Data;
using Backend.Services;

public interface ISubtitleConverter
{
    Task ConvertSpeechBubble(SpeechBubble speechBubble);
}
