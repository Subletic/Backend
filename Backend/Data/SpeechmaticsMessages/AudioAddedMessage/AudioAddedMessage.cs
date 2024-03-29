#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Data.SpeechmaticsMessages.AudioAddedMessage;

/// <summary>
/// Speechmatics RT API message: AudioAdded
/// Direction: Server -> Client
/// When:
/// - after <c>StartRecognitionMessage</c> -> <c>RecognitionStartedMessage</c> handshake
/// - Server received an <c>AddAudio</c> message (raw audio data)
/// Purpose: Signal that Server has acknowledged sent audio data and started processing it
/// Effects:
/// - one of the following <c>AddTranscriptMessage</c>s will contain a transcript of the recognised speech
/// - <c>seq_no</c> shows which <c>AddAudio</c> has been acknowledged
/// API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#audioadded" />
/// <see cref="seq_no" />
/// <see cref="AddTranscriptMessage" />
/// <seealso cref="StartRecognitionMessage" />
/// <seealso cref="RecognitionStartedMessage" />
/// </summary>
public class AudioAddedMessage
{
    /// <summary>
    /// Simple constructor.
    /// <param name="seq_no"><c>seq_no</c> that was acknowledged.</param>
    /// <see cref="seq_no" />
    /// </summary>
    /// <param name="seq_no">The id of the <c>AddAudio</c> that has been acknowledged.</param>
    public AudioAddedMessage(ulong seq_no = 0)
    {
        this.seq_no = seq_no;
    }

    /// <summary>
    /// The internal name of this message.
    /// </summary>
    private const string MESSAGE_TYPE = "AudioAdded";

    /// <summary>
    /// Gets or sets the message name.
    /// Settable for JSON deserialising purposes, but value
    /// *MUST* match <c>MESSAGE_TYPE</c> when attempting to set.
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
    /// Gets or sets the id of the <c>AddAudio</c> that has been acknowledged.
    /// </summary>
    public ulong seq_no { get; set; }
}
