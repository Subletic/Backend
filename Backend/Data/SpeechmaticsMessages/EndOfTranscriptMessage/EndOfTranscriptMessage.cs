namespace Backend.Data.SpeechmaticsMessages.EndOfTranscriptMessage;

/**
  *  <summary>
  *  Speechmatics RT API message: EndOfTranscript
  *
  *  Direction: Server -> Client
  *  When:
  *  - after <c>StartRecognitionMessage</c> -> <c>RecognitionStartedMessage</c> handshake
  *  - Server received an <c>EndOfStreamMessage</c> and is done sending all transcripts
  *  Purpose: Signal that Server has finished sending all audio transcripts that it intended to send.
  *  Effects:
  *  - no further <c>AddTranscriptMessage</c>s are sent
  *  - connection may be safely terminated
  *
  *  <see cref="AddTranscriptMessage" />
  *  <see cref="EndOfStreamMessage" />
  *  <seealso cref="StartRecognitionMessage" />
  *  <seealso cref="RecognitionStartedMessage" />
  *  </summary>
  */
public class EndOfTranscriptMessage
{
    /**
      *  <summary>
      *  Simple constructor.
      *  </summary>
      */
    public EndOfTranscriptMessage () { }

    /**
      *  <summary>
      *  The internal name of this message.
      *  </summary>
      */
    private static readonly string _message = "EndOfTranscript";

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
}
