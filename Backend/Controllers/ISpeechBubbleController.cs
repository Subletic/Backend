using Backend.Data;

namespace Backend.Controllers;

public interface ISpeechBubbleController
{
    public void HandleNewWord(WordToken wordToken);
    public LinkedList<SpeechBubble> GetSpeechBubbles();
}