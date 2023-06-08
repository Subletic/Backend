using Backend.Data;

namespace Backend.Services;

public class SpeechBubbleListService : ISpeechBubbleListService
{
    private readonly LinkedList<SpeechBubble> _speechBubbleList;

    public SpeechBubbleListService()
    {
        _speechBubbleList = new LinkedList<SpeechBubble>();
    }

    public LinkedList<SpeechBubble> GetSpeechBubbles()
    {
        return _speechBubbleList;
    }

    public void AddNewSpeechBubble(SpeechBubble speechBubble)
    {
        _speechBubbleList.AddLast(speechBubble);
    }

    public void DeleteOldestSpeechBubble()
    {
        _speechBubbleList.RemoveFirst();
    }

    public void ReplaceSpeechBubble(SpeechBubble speechBubble)
    {
        var newSpeechBubbleId = speechBubble.Id;

        ReplaceInLinkedList(speechBubble, newSpeechBubbleId);
    }

    private void ReplaceInLinkedList(SpeechBubble speechBubble, long newSpeechBubbleId)
    {
        for (var i = 0; i < _speechBubbleList.Count; i++)
        {
            var currentSpeechBubbleId = _speechBubbleList.ElementAt(i).Id;

            if (newSpeechBubbleId != currentSpeechBubbleId) continue;

            _speechBubbleList.Remove(_speechBubbleList.ElementAt(i));

            if (_speechBubbleList.Count == 0 || i == 0)
            {
                _speechBubbleList.AddFirst(speechBubble);
                return;
            }

            var oldElement = _speechBubbleList.ElementAt(i - 1);
            _speechBubbleList.AddAfter(_speechBubbleList.Find(oldElement)!, speechBubble);
            return;
        }
    }
}