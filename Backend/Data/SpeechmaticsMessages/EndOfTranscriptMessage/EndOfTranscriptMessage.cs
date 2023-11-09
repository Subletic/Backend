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
  *  API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#endoftranscript" />
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
    private const string MESSAGE_TYPE = "EndOfTranscript";

    /**
      *  <value>
      *  Message name.
      *  Settable for JSON deserialising purposes, but value
      *  *MUST* match <c>MESSAGE_TYPE</c> when attempting to set.
      *  </value>
      */
    public string message
    {
        get { return MESSAGE_TYPE; }
        set
        {
            if (value != MESSAGE_TYPE)
                throw new ArgumentException (String.Format (
                    "wrong message type: expected {0}, received {1}",
                    MESSAGE_TYPE, value));
        }
    }
}
