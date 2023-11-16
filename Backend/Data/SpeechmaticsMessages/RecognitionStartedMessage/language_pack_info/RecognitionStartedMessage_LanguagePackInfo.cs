#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Data.SpeechmaticsMessages.RecognitionStartedMessage.language_pack_info;

/// <summary>
/// Metadata about the language being used for transcription.
/// API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#recognitionstarted" />
/// (no separate section, just included in its parent message's section)
/// <see cref="adapted" />
/// <see cref="itn" />
/// <see cref="language_description" />
/// <see cref="word_delimiter" />
/// <see cref="writing_direction" />
/// </summary>
public class RecognitionStartedMessage_LanguagePackInfo
{
    /// <summary>
    /// Simple constructor.
    /// <see cref="adapted" />
    /// <see cref="itn" />
    /// <see cref="language_description" />
    /// <see cref="word_delimiter" />
    /// <see cref="writing_direction" />
    /// </summary>
    /// <param name="adapted">Whether the language pack is adapted with Language Model Adaptation.</param>
    /// <param name="itn">Whether Inverse Text Normalization (ITN) is available for this language.</param>
    /// <param name="language_description">The full name of the language.</param>
    /// <param name="word_delimiter">The character to put between words.</param>
    /// <param name="writing_direction"><c>left-to-right</c> or <c>right-to-left</c></param>
    public RecognitionStartedMessage_LanguagePackInfo(
        bool adapted,
        bool itn,
        string language_description,
        string word_delimiter,
        string writing_direction)
    {
        this.adapted = adapted;
        this.itn = itn;
        this.language_description = language_description;
        this.word_delimiter = word_delimiter;
        this.writing_direction = writing_direction;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the language pack is adapted with Language Model Adaptation (an upcoming feature).
    /// </summary>
    public bool adapted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Inverse Text Normalization (ITN) is available for this language.
    /// ITN improves the formatting of entities in the text such as numerals and dates.
    /// </summary>
    public bool itn { get; set; }

    /// <summary>
    /// Gets or sets the full name of the language.
    /// </summary>
    public string language_description { get; set; }

    /// <summary>
    /// Gets or sets the character to put between words.
    /// </summary>
    public string word_delimiter { get; set; }

    /// <summary>
    /// Gets or sets the writing direction.
    /// <c>left-to-right</c> or <c>right-to-left</c>
    /// </summary>
    public string writing_direction { get; set; }
}
