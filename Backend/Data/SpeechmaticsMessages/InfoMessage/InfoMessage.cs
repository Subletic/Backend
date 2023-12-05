#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Data.SpeechmaticsMessages.InfoMessage;

/// <summary>
/// Speechmatics RT API message: Info
/// Direction: Server -> Client
/// When: After <c>StartRecognitionMessage</c>(?), depends on <c>type</c>
/// Purpose: Additional information about something
/// Effects: Various, depends on <c>type</c>
/// API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#info" />
/// <see cref="type" />
/// <seealso cref="StartRecognitionMessage" />
/// </summary>
public class InfoMessage
{
    /// <summary>
    /// Simple constructor.
    /// <see cref="code" />
    /// <see cref="type" />
    /// <see cref="reason" />
    /// <see cref="quality" />
    /// </summary>
    /// <param name="code">A numerical code for the info message.</param>
    /// <param name="type">A code for the info message.</param>
    /// <param name="reason">A human-readable reason for the info message.</param>
    /// <param name="quality">
    /// In case of <c>recognition_quality</c>, the name of the quality-based model that will be used.
    /// </param>
    public InfoMessage(int? code, string type, string reason, string? quality)
    {
        this.code = code;
        this.type = type;
        this.reason = reason;
        this.quality = quality;
    }

    /// <summary>
    /// The internal name of this message.
    /// </summary>
    private const string MESSAGE_TYPE = "Info";

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
    /// Gets or sets an optional numerical code for the info message.
    /// </summary>
    public int? code { get; set; }

    /// <summary>
    /// Gets or sets a string code for the info message.
    /// <c>recognition_quality</c>:
    /// - When: After <c>StartRecognitionMessage</c>
    /// - Effects: n/a
    /// <c>model_redirect</c>:
    /// - When: After <c>StartRecognitionMessage</c>, if a deprecated model language code was specified
    /// - Effects: A different recognition model will handle the data
    /// <c>deprecated</c>:
    /// - When: ?, when a deprecated feature has been requested
    /// - Effects: In the future, the feature in question will be removed
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// Gets or sets a human-readable reason for the info message.
    /// </summary>
    public string reason { get; set; }

    /// <summary>
    /// Gets or sets an optional string code for the info message.
    /// If <c>recognition_quality</c>, the name of the quality-based model that will be used. For example:
    /// - <c>telephony</c>: low-quality audio
    /// - <c>broadcast</c>: high-quality audio (>12kHz)
    /// </summary>
    public string? quality { get; set; }
}
