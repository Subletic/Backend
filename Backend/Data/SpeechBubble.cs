namespace Backend.Data;

public struct SpeechBubble
{
    public SpeechBubble(long id, int speaker, int start, int end, List<WordToken> wordTokens)
    {
        Id = id;
        Speaker = speaker;
        Start = start;
        End = end;
        SpeechBubbleContent = wordTokens;
    }
    
    public long Id { get; set; }
    public int Speaker { get; set; }
    public int Start { get; set; }
    public int End { get; set; }
    public List<WordToken> SpeechBubbleContent { get; set; }
}