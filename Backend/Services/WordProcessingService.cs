using Backend.Data;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Services;

/// <summary>
/// Service used for inserting new WordTokens into data-structure
/// </summary>
public class WordProcessingService : IWordProcessingService
{
    /// <summary>
    /// Dependency Injection for accessing the LinkedList of SpeechBubbles and corresponding methods.
    /// All actions on the SpeechBubbleList are delegated to the SpeechBubbleListService.
    /// </summary>
    private readonly ISpeechBubbleListService _speechBubbleListService;

    private readonly IHubContext<CommunicationHub> _hubContext;
    private readonly List<WordToken> _wordTokenBuffer;

    private long _nextSpeechBubbleId;
    private int? _currentSpeaker;

    public WordProcessingService(IHubContext<CommunicationHub> hubContext,
        ISpeechBubbleListService speechBubbleListService)
    {
        _wordTokenBuffer = new List<WordToken>();
        _nextSpeechBubbleId = 1;
        _hubContext = hubContext;
        _speechBubbleListService = speechBubbleListService;
    }


    /// <summary>
    /// Handles new WordToken given by the Speech-Recognition Software or Mock-Server.
    /// WordTokens are added to a local buffer, which is flushed when the conditions for a new SpeechBubble are met.
    /// Conditions for a new SpeechBubble are contained in the IsSpeechBubbleFull() method.
    /// </summary>
    /// <param name="wordToken">The Word Token that should be appended to a SpeechBubble</param>
    public void HandleNewWord(WordToken wordToken)
    {
        Console.WriteLine($"New word: {wordToken.Word}, confidence {wordToken.Confidence}");
        SetSpeakerIfSpeakerIsNull(wordToken);

        // Append point or comma to last WordToken
        if (wordToken.Word is "." or ",")
        {
            switch (_wordTokenBuffer.Count)
            {
                case 0 when _speechBubbleListService.GetSpeechBubbles().Count > 0:
                {
                    var lastSpeechBubble = _speechBubbleListService.GetSpeechBubbles().Last();
                    lastSpeechBubble.SpeechBubbleContent.Last().Word += wordToken.Word;
                    return;
                }
                case > 0:
                    _wordTokenBuffer.Last().Word += wordToken.Word;
                    return;
            }
        }

        // Add new word Token to current Speech Bubble
        if (_currentSpeaker != null && _currentSpeaker == wordToken.Speaker)
        {
            if (IsSpeechBubbleFull(wordToken))
            {
                FlushBufferToNewSpeechBubble();
            }

            _wordTokenBuffer.Add(wordToken);
        }
        // Finish current SpeechBubble if new Speaker is detected
        else if (_currentSpeaker != null && _currentSpeaker != wordToken.Speaker)
        {
            FlushBufferToNewSpeechBubble();

            _currentSpeaker = wordToken.Speaker;
            _wordTokenBuffer.Add(wordToken);
        }
    }

    /// <summary>
    /// Method to check if the current SpeechBubble is full.
    /// Conditions for determining that a SpeechBubble is full are:
    ///     - A new Speaker is detected
    ///     - The buffer contains 20 words
    ///     - Two consecutive words have a time difference of at least 5 seconds.
    ///
    /// These conditions are held in the constants maxWordCount and maxSecondsTimeDifference and are subject to change.
    /// </summary>
    /// <param name="wordToken">The new Word Token received from the Mock-Server or Speech-Recognition Software</param>
    /// <returns>True if new WordToken should be in a new SpeechBubble</returns>
    private bool IsSpeechBubbleFull(WordToken wordToken)
    {
        const int maxWordCount = 20;
        const int maxSecondsTimeDifference = 5;
        var isTimeLimitExceeded = false;

        if (_wordTokenBuffer.Count > 0)
        {
            var lastBufferElementTimeStamp = _wordTokenBuffer.Last().EndTime;
            var newWordTokenTimeStamp = wordToken.StartTime;
            var timeDifference = newWordTokenTimeStamp - lastBufferElementTimeStamp;

            isTimeLimitExceeded = timeDifference > maxSecondsTimeDifference;
        }

        return _wordTokenBuffer.Count >= maxWordCount - 1 || isTimeLimitExceeded;
    }

    /// <summary>
    /// Method to flush the WordToken buffer to a new SpeechBubble.
    /// Empties WordBuffer and appends new SpeechBubble to the End of the LinkedList
    /// Increments the SpeechBubbleId by 1, so each SpeechBubble has a unique Id.
    /// </summary>
    private async void FlushBufferToNewSpeechBubble()
    {
        var nextSpeechBubble = new SpeechBubble(
            id: _nextSpeechBubbleId,
            speaker: (int)_currentSpeaker!,
            startTime: _wordTokenBuffer.First().StartTime,
            endTime: _wordTokenBuffer.Last().EndTime,
            wordTokens: new List<WordToken>(_wordTokenBuffer)
        );

        _nextSpeechBubbleId++;
        _speechBubbleListService.AddNewSpeechBubble(nextSpeechBubble);
        _wordTokenBuffer.Clear();

        await SendNewSpeechBubbleMessageToFrontend(nextSpeechBubble);
    }

    /// <summary>
    /// Method to set the current Speaker if the current Speaker is null.
    /// Should only be called for the first SpeechBubble
    /// </summary>
    /// <param name="wordToken">The new Word Token received from the Mock-Server or Speech-Recognition Software</param>
    private void SetSpeakerIfSpeakerIsNull(WordToken wordToken)
    {
        _currentSpeaker ??= wordToken.Speaker;
    }

    /// <summary>
    /// Sends an asynchronous request to the frontend via SignalR.
    /// The frontend can then subscribe to incoming Objects and handle them accordingly.
    /// </summary>
    /// <param name="speechBubble"></param>
    private async Task SendNewSpeechBubbleMessageToFrontend(SpeechBubble speechBubble)
    {
        var listToSend = new List<SpeechBubble>() { speechBubble };

        try
        {
            await _hubContext.Clients.All.SendAsync("newBubble", listToSend);
        }
        catch (Exception)
        {
            await Console.Error.WriteAsync("Failed to transmit to Frontend.");
        }
    }
}