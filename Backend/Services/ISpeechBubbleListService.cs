using Backend.Data;

namespace Backend.Services;

public interface ISpeechBubbleListService
{
    public LinkedList<SpeechBubble> GetSpeechBubbles();

    public void AddNewSpeechBubble(SpeechBubble speechBubble);

    public void DeleteOldestSpeechBubble();

    public void ReplaceSpeechBubble(SpeechBubble speechBubble);

    



}