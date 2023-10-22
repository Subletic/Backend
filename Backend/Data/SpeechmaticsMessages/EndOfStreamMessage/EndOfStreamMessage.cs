namespace Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;

/**
  *  <summary>
  *  Speechmatics RT API message: EndOfStream
  *
  *  Direction: Client -> Server
  *  When: Client is done sending audio data
  *  Purpose: Signal that Client has finished sending all audio that it intended to send.
  *  Effects:
  *  - no further <c>AddAudio</c> messages are accepted
  *  - will respond with <c>EndOfTranscriptMessage</c>
  *  API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#endofstream" />
  *
  *  <see cref="EndOfTranscriptMessage" />
  *  </summary>
  */
public class EndOfStreamMessage
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="last_seq_no"><c>last_seq_no</c> to send.</param>
      *
      *  <see cref="last_seq_no" />
      *  </summary>
      */
    public EndOfStreamMessage (ulong last_seq_no = 0)
    {
        this.last_seq_no = last_seq_no;
    }

    /**
      *  <summary>
      *  The internal name of this message.
      *  </summary>
      */
    private static readonly string _message = "EndOfStream";

    /**
      *  <value>
      *  Message name.
      *  Settable for JSON deserialising purposes, but value
      *  *MUST* match <c>_message</c> when attempting to set.
      *  </value>
      */
    public string message
    {
        get { return _message; }
        set
        {
            if (value != _message)
                throw new ArgumentException (String.Format (
                    "wrong message type: expected {0}, received {1}",
                    _message, value));
        }
    }

    /**
      *  <value>
      *  The total number of audio chunks sent.
      *  </value>
      */
    public ulong last_seq_no { get; set; }
}
