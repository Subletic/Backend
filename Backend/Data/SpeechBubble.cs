using System.Transactions;

namespace Backend.Data;

public class SpeechBubble
{
    public SpeechBubble(long id, int speaker, double startTime, double endTime, List<WordToken> wordTokens)
    {
        Id = id;
        Speaker = speaker;
        StartTime = startTime;
        EndTime = endTime;
        SpeechBubbleContent = wordTokens;
        CreationTime = DateTime.Now;
    }
    
    public long Id { get; init; }
    public int Speaker { get; set; }
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public DateTime CreationTime { get; set; }
    public List<WordToken> SpeechBubbleContent { get; set; }
}