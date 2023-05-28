using Backend.Data;

namespace Backend.Controllers;

public class SpeechBubbleController
{
    private readonly LinkedList<SpeechBubble> _speechBubbleList;
    private readonly List<WordToken> _wordTokenBuffer;

    private long _nextSpeechBubbleId;
    private int? _currentSpeaker;

    public SpeechBubbleController()
    {
        _speechBubbleList = new LinkedList<SpeechBubble>();
        _wordTokenBuffer = new List<WordToken>();
        _nextSpeechBubbleId = 1;
    }

    public void HandleNewWord(WordToken wordToken)
    {
        const int maxWordCount = 20;
        
        SetSpeakerIfSpeakerIsNull(wordToken);

        // Add new word Token to current Speech Bubble
        if (_currentSpeaker != null && _currentSpeaker == wordToken.Speaker)
        {
            _wordTokenBuffer.Add(wordToken);
            
            if (_wordTokenBuffer.Count < maxWordCount) return;
            FlushBufferToNewSpeechBubble();
        }
        // Finish current SpeechBubble if new Speaker is detected
        else if (_currentSpeaker != null && _currentSpeaker != wordToken.Speaker)
        {
            FlushBufferToNewSpeechBubble();

            _currentSpeaker = wordToken.Speaker;
        }
    }

    private void FlushBufferToNewSpeechBubble()
    {
        var nextSpeechBubble = new SpeechBubble(
            id: _nextSpeechBubbleId,
            speaker: (int)_currentSpeaker!,
            start: _wordTokenBuffer.First().TimeStamp,
            end: _wordTokenBuffer.Last().TimeStamp,
            wordTokens: _wordTokenBuffer
        );

        _nextSpeechBubbleId++;
        _wordTokenBuffer.Clear();
        _speechBubbleList.AddLast(nextSpeechBubble);
    }

    private void SetSpeakerIfSpeakerIsNull(WordToken wordToken)
    {
        _currentSpeaker ??= wordToken.Speaker;
    }

    public LinkedList<SpeechBubble> GetSpeechBubbles()
    {
        return _speechBubbleList;
    }
}