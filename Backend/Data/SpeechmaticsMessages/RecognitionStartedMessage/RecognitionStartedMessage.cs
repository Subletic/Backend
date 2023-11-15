#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Data.SpeechmaticsMessages.RecognitionStartedMessage;

using Backend.Data.SpeechmaticsMessages.RecognitionStartedMessage.language_pack_info;

/// <summary>
/// Speechmatics RT API message: RecognitionStarted
/// Direction: Server -> Client
/// When: After <c>StartRecognitionMessage</c>
/// Purpose: Signal that Server has started the transcription, and inform about the settings and details
/// Effects:
/// - Transcription has begun
/// - <c>AddAudio</c> messages and <c>EndOfStreamMessage</c>s will now be accepted
/// API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#recognitionstarted" />
/// <see cref="EndOfStreamMessage" />
/// <see cref="StartRecognitionMessage" />
/// </summary>
public class RecognitionStartedMessage
{
    /// <summary>
    /// Simple constructor.
    /// <see cref="id" />
    /// <see cref="language_pack_info" />
    /// <see cref="RecognitionStartedMessage_LanguagePackInfo" />
    /// </summary>
    /// <param name="id">GUID that identifies the session.</param>
    /// <param name="language_pack_info">Metadata about the language being used for transcription.</param>
    public RecognitionStartedMessage(string id, RecognitionStartedMessage_LanguagePackInfo language_pack_info)
    {
        this.id = id;
        this.language_pack_info = language_pack_info;
    }

    /// <summary>
    /// The internal name of this message.
    /// </summary>
    private const string MESSAGE_TYPE = "RecognitionStarted";

    /// <summary>
    /// Gets or sets the message name.
    /// Settable for JSON deserializing purposes, but value
    /// *MUST*match <c>MESSAGE_TYPE</c> when attempting to set.
    /// </summary>
    public string message
    {
        get
        {
            return MESSAGE_TYPE;
        }

        set
        {
            if (value != MESSAGE_TYPE)
                throw new ArgumentException(string.Format("wrong message type: expected {0}, received {1}", MESSAGE_TYPE, value));
        }
    }

    /// <summary>
    /// Gets or sets a randomly-generated GUID that identifies the session.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Gets or sets metadata about the language being used for transcription.
    /// </summary>
    public RecognitionStartedMessage_LanguagePackInfo language_pack_info { get; set; }
}
