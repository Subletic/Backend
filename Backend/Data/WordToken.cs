namespace Backend.Data;

public struct WordToken
{
    public WordToken(string word, float confidence, int timeStamp, int speaker)
    {
        Word = word;
        Confidence = confidence;
        TimeStamp = timeStamp;
        Speaker = Speaker;
    }
    
    public string Word { get; set; }
    public float Confidence { get; set; }
    public int TimeStamp { get; set; }
    public int Speaker { get; set; }
}