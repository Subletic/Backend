namespace Backend.Data;

public struct WordToken
{
    public WordToken(string word, float confidence, double startTime, double endTime, int speaker)
    {
        Word = word;
        Confidence = confidence;
        StartTime = startTime;
        EndTime = endTime;
        Speaker = speaker;
    }
    
    public string Word { get; set; }
    public float Confidence { get; set; }
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public int Speaker { get; set; }
}