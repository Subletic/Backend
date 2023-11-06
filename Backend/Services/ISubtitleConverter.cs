using Backend.Data;
using Backend.Services;

namespace Backend.Services;

public interface ISubtitleConverter
{
    void ConvertSpeechBubble(SpeechBubble speechBubble);
}
