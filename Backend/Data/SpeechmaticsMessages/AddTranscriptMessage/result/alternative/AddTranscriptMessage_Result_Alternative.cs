#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result.alternative;

/// <summary>
/// A possible word/symbol.
/// API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#addtranscript" />
/// (no separate section, just included in its parent message's section)
/// <see cref="content" />
/// <see cref="confidence" />
/// <see cref="language" />
/// <see cref="speaker" />
/// </summary>
public class AddTranscriptMessage_Result_Alternative
{
    /// <summary>
    /// Simple constructor.
    /// <see cref="content" />
    /// <see cref="confidence" />
    /// <see cref="language" />
    /// <see cref="speaker" />
    /// </summary>
    /// <param name="content">A word or punctuation mark.</param>
    /// <param name="confidence">A confidence score assigned to the alternative, from 0.0 to 1.0 (worst to
    /// best).</param>
    /// <param name="language">The language that the alternative word is assumed to be spoken in.</param>
    /// <param name="speaker">Diarisation label indicating who said that word.</param>
    public AddTranscriptMessage_Result_Alternative(
        string content,
        double confidence,
        string? language = null,
        string? speaker = null)
    {
        this.content = content;
        this.confidence = confidence;
        this.language = language;
        this.speaker = speaker;
    }

    /// <summary>
    /// Gets or sets the word or punctuation mark.
    /// </summary>
    public string content { get; set; }

    /// <summary>
    /// Gets or sets a confidence score assigned to the alternative.
    /// Ranges from 0.0 (least confident) to 1.0 (most confident).
    /// </summary>
    public double confidence { get; set; }

    /// <summary>
    /// Gets or sets the language that the alternative word is assumed to be spoken in.
    /// Currently, this will always be equal to the language that was requested
    /// in the initial <c>StartRecognitionMessage</c>.
    /// </summary>
    public string? language { get; set; }

    /// <summary>
    /// Gets or sets the label indicating who said that word. Only set if diarization is enabled.
    /// </summary>
    public string? speaker { get; set; }
}
