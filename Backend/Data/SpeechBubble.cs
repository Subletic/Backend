using System.Transactions;

namespace Backend.Data;

public struct SpeechBubble
{
    public SpeechBubble(long id, int speaker, double startTime, double endTime, List<WordToken> wordTokens)
    {
        Id = id;
        Speaker = speaker;
        StartTime = startTime;
        EndTime = endTime;
        SpeechBubbleContent = wordTokens;
    }
    
    public long Id { get; set; }
    public int Speaker { get; set; }
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public List<WordToken> SpeechBubbleContent { get; set; }
}