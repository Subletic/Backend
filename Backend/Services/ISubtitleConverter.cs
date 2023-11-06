using Backend.Data;
using Backend.Services;

public interface ISubtitleConverter
{
    void ConvertSpeechBubble(SpeechBubble speechBubble);
}
