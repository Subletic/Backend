namespace Backend.Data;

/// <summary>
/// Struct used for receiving updated speech bubbles from the frontend.
/// </summary>
public struct SpeechBubbleJson
{
    public long Id { get; init; }
    public int Speaker { get; set; }
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public List<WordToken> SpeechBubbleContent { get; set; }

    public SpeechBubbleJson(long id, int speaker, double startTime, double endTime, List<WordToken> wordTokens)
    {
        Id = id;
        Speaker = speaker;
        StartTime = startTime;
        EndTime = endTime;
        SpeechBubbleContent = wordTokens;
    }
}