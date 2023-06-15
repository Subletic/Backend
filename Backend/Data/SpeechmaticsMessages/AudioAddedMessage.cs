namespace Backend.Data.SpeechmaticsMessages;

/**
  *  <summary>
  *  Speechmatics RT API message: AudioAdded
  *
  *  Direction: Server -> Client
  *  When:
  *  - after <c>StartRecognitionMessage</c> -> <c>RecognitionStartedMessage</c> handshake
  *  - Server received an <c>AddAudio</c> message (raw audio data)
  *  Purpose: Signal that Server has acknowledged sent audio data and started processing it
  *  Effects:
  *  - one of the following <c>AddTranscriptMessage</c>s will contain a transcript of the recognised speech
  *  - <c>seq_no</c> shows which <c>AddAudio</c> has been acknowledged
  *
  *  <see cref="seq_no" />
  *  <see cref="AddTranscriptMessage" />
  *  <seealso cref="StartRecognitionMessage" />
  *  <seealso cref="RecognitionStartedMessage" />
  *  </summary>
  */
public class AudioAddedMessage
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="seq_no"><c>seq_no</c> that was acknowledged.</param>
      *
      *  <see cref="seq_no" />
      *  </summary>
      */
    public AudioAddedMessage (ulong seq_no = 0)
    {
        this.seq_no = seq_no;
    }

    /**
      *  <summary>
      *  The internal name of this message.
      *  </summary>
      */
    private static readonly string _message = "AudioAdded";

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
      *  The id of the <c>AddAudio</c> that has been acknowledged.
      *  </value>
      */
    public ulong seq_no { get; set; }
}
