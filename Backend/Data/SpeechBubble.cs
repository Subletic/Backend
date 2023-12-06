namespace Backend.Data;

using System.Transactions;

/// <summary>
/// Represents a speech bubble containing a list of word tokens spoken by a speaker within a certain time frame.
/// </summary>
public class SpeechBubble
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechBubble"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the speech bubble.</param>
    /// <param name="speaker">The identifier of the speaker.</param>
    /// <param name="startTime">The start time of the speech bubble.</param>
    /// <param name="endTime">The end time of the speech bubble.</param>
    /// <param name="wordTokens">The list of word tokens contained in the speech bubble.</param>
    public SpeechBubble(long id, int speaker, double startTime, double endTime, List<WordToken> wordTokens)
    {
        Id = id;
        Speaker = speaker;
        StartTime = startTime;
        EndTime = endTime;
        SpeechBubbleContent = wordTokens;
        CreationTime = DateTime.Now;
    }

    /// <summary>
    /// Gets the unique identifier of the speech bubble.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the speaker.
    /// </summary>
    public int Speaker { get; set; }

    /// <summary>
    /// Gets or sets the start time of the speech bubble.
    /// </summary>
    public double StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the speech bubble.
    /// </summary>
    public double EndTime { get; set; }

    /// <summary>
    /// Gets or sets the creation time of the speech bubble.
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the list of word tokens contained in the speech bubble.
    /// </summary>
    public List<WordToken> SpeechBubbleContent { get; set; }
}
