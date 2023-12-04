namespace Backend.Data;

/// <summary>
/// Struct used for receiving List of updated SpeechBubbles from the frontend.
/// </summary>
public struct SpeechBubbleChainJson
{
    /// <summary>
    /// Gets or sets the list of speech bubbles in the chain.
    /// </summary>
    public List<SpeechBubbleJson>? SpeechbubbleChain { get; set; }

    /// <summary>
    /// Represents a JSON object that contains a list of speech bubbles.
    /// </summary>
    /// <param name="postSpeechBubblesList">The list of speech bubbles in the chain.</param>
    public SpeechBubbleChainJson(List<SpeechBubbleJson> postSpeechBubblesList)
    {
        SpeechbubbleChain = postSpeechBubblesList;
    }

    /// <summary>
    /// Parses incoming JSON from the frontend to a list of backend-compatible SpeechBubbles.
    /// </summary>
    /// <param name="receivedList">The non-empty json received</param>
    /// <returns>List of parsed SpeechBubbles</returns>
    public List<SpeechBubble> ToSpeechBubbleList()
    {
        var speechBubblesList = new List<SpeechBubble>();

        foreach (var currentJsonSpeechBubble in SpeechbubbleChain!)
        {
            List<WordToken> wordTokens = jsonSpeechBubbleToWordTokens(currentJsonSpeechBubble);

            speechBubblesList.Add(new SpeechBubble(
                currentJsonSpeechBubble.Id,
                currentJsonSpeechBubble.Speaker,
                currentJsonSpeechBubble.StartTime,
                currentJsonSpeechBubble.EndTime,
                wordTokens));
        }

        return speechBubblesList;
    }

    private static List<WordToken> jsonSpeechBubbleToWordTokens(SpeechBubbleJson jsonSpeechBubbles)
    {
        var receivedWordTokens = new List<WordToken>();
        foreach (var currentWordToken in jsonSpeechBubbles.SpeechBubbleContent)
        {
            receivedWordTokens.Add(new WordToken(
                currentWordToken.Word,
                currentWordToken.Confidence,
                currentWordToken.StartTime,
                currentWordToken.EndTime,
                currentWordToken.Speaker));
        }

        return receivedWordTokens;
    }
}
