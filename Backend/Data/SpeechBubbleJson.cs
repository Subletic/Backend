namespace Backend.Data;

/// <summary>
/// Struct used for receiving updated speech bubbles from the frontend.
/// </summary>
public struct SpeechBubbleJson
{
    /// <summary>
    /// Gets the ID of the speech bubble.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets or sets the speaker of the speech bubble.
    /// </summary>
    public int Speaker { get; set; }

    /// <summary>
    /// Gets or sets the start time of the speech bubble in seconds.
    /// </summary>
    public double StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the speech bubble in seconds.
    /// </summary>
    public double EndTime { get; set; }

    /// <summary>
    /// Gets or sets the start time of the speech bubble in seconds.
    /// </summary>
    public List<WordToken> SpeechBubbleContent { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechBubbleJson"/> struct.
    /// </summary>
    /// <param name="id">The ID of the speech bubble.</param>
    /// <param name="speaker">The speaker of the speech bubble.</param>
    /// <param name="startTime">The start time of the speech bubble in seconds.</param>
    /// <param name="endTime">The end time of the speech bubble in seconds.</param>
    /// <param name="wordTokens">The word tokens of the speech bubble.</param>
    public SpeechBubbleJson(long id, int speaker, double startTime, double endTime, List<WordToken> wordTokens)
    {
        Id = id;
        Speaker = speaker;
        StartTime = startTime;
        EndTime = endTime;
        SpeechBubbleContent = wordTokens;
    }
}
