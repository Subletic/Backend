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
}
