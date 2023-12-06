#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;

/// <summary>
/// Speechmatics RT API message: EndOfStream
/// Direction: Client -> Server
/// When: Client is done sending audio data
/// Purpose: Signal that Client has finished sending all audio that it intended to send.
/// Effects:
/// - no further <c>AddAudio</c> messages are accepted
/// - will respond with <c>EndOfTranscriptMessage</c>
/// API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#endofstream" />
/// <see cref="EndOfTranscriptMessage" />
/// </summary>
public class EndOfStreamMessage
{
    /// <summary>
    /// Simple constructor.
    /// <see cref="last_seq_no" />
    /// </summary>
    /// <param name="last_seq_no"><c>last_seq_no</c> to send.</param>
    public EndOfStreamMessage(ulong last_seq_no = 0)
    {
        this.last_seq_no = last_seq_no;
    }

    /// <summary>
    /// The internal name of this message.
    /// </summary>
    private const string MESSAGE_TYPE = "EndOfStream";

    /// <summary>
    /// Gets or sets the message name.
    /// Settable for JSON deserialising purposes, but value
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
    /// Gets or sets the total number of audio chunks sent.
    /// </summary>
    public ulong last_seq_no { get; set; }
}
