#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

/// <summary>
/// Configuration for this recognition session.
/// API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#transcription-config" />
/// <see cref="language" />
/// <see cref="enable_partials" />
/// <see cref="additionalVocab" />
/// </summary>
public class StartRecognitionMessage_TranscriptionConfig
{
    /// <summary>
    /// Maximum count for additional vocabulary.
    /// </summary>
    public const int MAX_ADDITIONAL_VOCAB_COUNT = 1000;

    /// <summary>
    /// Simple constructor.
    /// <see cref="language" />
    /// <see cref="enable_partials" />
    /// <see cref="additionalVocab" />
    /// </summary>
    /// <param name="language">Language model to process audio, usually in ISO language code format.</param>
    /// <param name="enable_partials">Whether to send partial transcripts (<c>AddPartialTranscript</c>
    /// messages).</param>
    /// <param name="additional_vocab">Additional vocabulary for transcription.</param>
    public StartRecognitionMessage_TranscriptionConfig(
        string language = "de",
        bool? enable_partials = false,
        List<AdditionalVocab>? additional_vocab = null)
    {
        this.language = language;
        this.enable_partials = enable_partials;
        this.additional_vocab = additional_vocab ?? new List<AdditionalVocab>();

        if (additional_vocab != null && additional_vocab.Count > MAX_ADDITIONAL_VOCAB_COUNT)
        {
            throw new ArgumentException($"additionalVocab list cannot exceed {MAX_ADDITIONAL_VOCAB_COUNT} elements.");
        }
    } // not sure how to validate that language is an ISO language code

    /// <summary>
    /// Gets or sets the language model to process audio, usually in ISO language code format.
    /// The value must be consistent with the language code used in the API endpoint URL.
    /// </summary>
    public string language { get; set; }

    /// <summary>
    /// Gets or sets whether to send partial transcripts (<c>AddPartialTranscript</c> messages)
    /// in addition to the finals (i.e. `AddTranscriptMessage`s).
    /// For now, the partials are somewhat useless to us. They are sent as a slowly accumulating list, each time
    /// resending the previous values together with the new ones. It might be possible to similarly accumulate values
    /// from partials in a special structure and only register the words that aren't resends.
    /// I don't know if there are any guarantees about the stability of transcriptions between partials. I think it's
    /// possible that the AI can go back and correct words in the partials, in which case these are wholly incompatible
    /// with our expectations. This could use some more testing.
    /// </summary>
    public bool? enable_partials { get; set; }

    /// <summary>
    /// Gets or sets additional vocabulary for transcription.
    /// </summary>
    public List<AdditionalVocab> additional_vocab { get; set; }
}
