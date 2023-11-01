using Backend.Data;
using Backend.Services;

public interface ISubtitleConverter
{
    string ConvertToWebVttFormat(SpeechBubble speechBubble);
    void ExportSpeechBubble(SpeechBubble speechBubble);
}
