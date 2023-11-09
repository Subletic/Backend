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
    private readonly ISpeechBubbleListService speechBubbleListService;

    private readonly IHubContext<CommunicationHub> hubContext;
    private readonly List<WordToken> wordTokenBuffer;

    private long nextSpeechBubbleId;
    private int? currentSpeaker;

    public WordProcessingService(IHubContext<CommunicationHub> hubContext,
        ISpeechBubbleListService speechBubbleListService)
    {
        this.wordTokenBuffer = new List<WordToken>();
        this.hubContext = hubContext;
        this.speechBubbleListService = speechBubbleListService;

        nextSpeechBubbleId = 1;
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
        if (wordToken.Word is "." or "," or "!" or "?")
        {
            switch (wordTokenBuffer.Count)
            {
                case 0 when speechBubbleListService.GetSpeechBubbles().Count > 0:
                {
                    var lastSpeechBubble = speechBubbleListService.GetSpeechBubbles().Last();
                    lastSpeechBubble.SpeechBubbleContent.Last().Word += wordToken.Word;
                    return;
                }
                case > 0:
                    wordTokenBuffer.Last().Word += wordToken.Word;
                    return;
            }
        }

        // Add new word Token to current Speech Bubble
        if (currentSpeaker != null && currentSpeaker == wordToken.Speaker)
        {
            if (IsSpeechBubbleFull(wordToken))
            {
                FlushBufferToNewSpeechBubble();
            }

            wordTokenBuffer.Add(wordToken);
        }
        // Finish current SpeechBubble if new Speaker is detected
        else if (currentSpeaker != null && currentSpeaker != wordToken.Speaker)
        {
            FlushBufferToNewSpeechBubble();

            currentSpeaker = wordToken.Speaker;
            wordTokenBuffer.Add(wordToken);
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

        if (wordTokenBuffer.Count > 0)
        {
            var lastBufferElementTimeStamp = wordTokenBuffer.Last().EndTime;
            var newWordTokenTimeStamp = wordToken.StartTime;
            var timeDifference = newWordTokenTimeStamp - lastBufferElementTimeStamp;

            isTimeLimitExceeded = timeDifference > maxSecondsTimeDifference;
        }

        return wordTokenBuffer.Count >= maxWordCount - 1 || isTimeLimitExceeded;
    }

    /// <summary>
    /// Method to flush the WordToken buffer to a new SpeechBubble.
    /// Empties WordBuffer and appends new SpeechBubble to the End of the LinkedList
    /// Increments the SpeechBubbleId by 1, so each SpeechBubble has a unique Id.
    /// </summary>
    private async void FlushBufferToNewSpeechBubble()
    {
        var nextSpeechBubble = new SpeechBubble(
            id: nextSpeechBubbleId,
            speaker: (int)currentSpeaker!,
            startTime: wordTokenBuffer.First().StartTime,
            endTime: wordTokenBuffer.Last().EndTime,
            wordTokens: new List<WordToken>(wordTokenBuffer)
        );

        nextSpeechBubbleId++;
        speechBubbleListService.AddNewSpeechBubble(nextSpeechBubble);
        wordTokenBuffer.Clear();

        await SendNewSpeechBubbleMessageToFrontend(nextSpeechBubble);
    }

    /// <summary>
    /// Method to set the current Speaker if the current Speaker is null.
    /// Should only be called for the first SpeechBubble
    /// </summary>
    /// <param name="wordToken">The new Word Token received from the Mock-Server or Speech-Recognition Software</param>
    private void SetSpeakerIfSpeakerIsNull(WordToken wordToken)
    {
        currentSpeaker ??= wordToken.Speaker;
    }

    /// <summary>
    /// Sends an asynchronous request to the frontend via SignalR, to publish a new Speechbubble.
    /// The frontend can then subscribe to incoming Objects and handle them accordingly.
    /// </summary>
    /// <param name="speechBubble"></param>
    private async Task SendNewSpeechBubbleMessageToFrontend(SpeechBubble speechBubble)
    {
        var listToSend = new List<SpeechBubble>() { speechBubble };

        try
        {
            await hubContext.Clients.All.SendAsync("newBubble", listToSend);
        }
        catch (Exception)
        {
            await Console.Error.WriteAsync("Failed to transmit to Frontend.");
        }
    }
}
