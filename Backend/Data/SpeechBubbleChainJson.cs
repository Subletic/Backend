namespace Backend.Data;

/// <summary>
/// Struct used for receiving List of updated SpeechBubbles from the frontend.
/// </summary>
public struct SpeechBubbleChainJson
{
    public List<SpeechBubbleJson>? SpeechbubbleChain { get; set; }

    public SpeechBubbleChainJson(List<SpeechBubbleJson> postSpeechBubblesList)
    {
        SpeechbubbleChain = postSpeechBubblesList;
    }
}