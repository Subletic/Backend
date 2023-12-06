namespace Backend.Data;

/// <summary>
/// Represents a single word in a transcript, including its associated metadata such as confidence, start and end times, and speaker.
/// </summary>
public class WordToken
{
    /// <summary>
    /// Represents a single word in a transcript with its associated metadata.
    /// </summary>
    /// <param name="word">The word.</param>
    /// <param name="confidence">The confidence.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="speaker">The speaker.</param>
    public WordToken(string word, float confidence, double startTime, double endTime, int speaker)
    {
        Word = word;
        Confidence = confidence;
        StartTime = startTime;
        EndTime = endTime;
        Speaker = speaker;
    }

    /// <summary>
    /// Gets or sets the word associated with this token.
    /// </summary>
    public string Word { get; set; }

    /// <summary>
    /// Gets or sets the confidence of the word.
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Gets or sets the start time of the word in seconds.
    /// </summary>
    public double StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the word in seconds.
    /// </summary>
    public double EndTime { get; set; }

    /// <summary>
    /// Gets or sets the speaker of the word.
    /// </summary>
    public int Speaker { get; set; }
}
