using Backend.Data.SpeechmaticsMessages.RecognitionStartedMessage.language_pack_info;

namespace Backend.Data.SpeechmaticsMessages.RecognitionStartedMessage;

/**
  *  <summary>
  *  Speechmatics RT API message: RecognitionStarted
  *
  *  Direction: Server -> Client
  *  When: After <c>StartRecognitionMessage</c>
  *  Purpose: Signal that Server has started the transcription, and inform about the settings and details
  *  Effects:
  *  - Transcription has begun
  *  - <c>AddAudio</c> messages and <c>EndOfStreamMessage</c>s will now be accepted
  *
  *  <see cref="EndOfStreamMessage" />
  *  <see cref="StartRecognitionMessage" />
  *  </summary>
  */
public class RecognitionStartedMessage
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="id">GUID that identifies the session.</param>
      *  <param name="language_pack_info">Metadata about the language being used for transcription.</param>
      *
      *  <see cref="id" />
      *  <see cref="language_pack_info" />
      *  <see cref="RecognitionStartedMessage_LanguagePackInfo" />
      *  </summary>
      */
    public RecognitionStartedMessage (string id, RecognitionStartedMessage_LanguagePackInfo language_pack_info)
    {
        this.id = id;
        this.language_pack_info = language_pack_info;
    }

    /**
      *  <summary>
      *  The internal name of this message.
      *  </summary>
      */
    private static readonly string _message = "RecognitionStarted";

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
      *  A randomly-generated GUID that identifies the session.
      *  </value>
      */
    public string id { get; set; }

    /**
      *  <value>
      *  Metadata about the language being used for transcription.
      *  </value>
      */
    public RecognitionStartedMessage_LanguagePackInfo language_pack_info { get; set; }
}
