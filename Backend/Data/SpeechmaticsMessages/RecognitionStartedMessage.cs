namespace Backend.Data.SpeechmaticsMessages;

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

/**
  *  <summary>
  *  Metadata about the language being used for transcription.
  *
  *  <see cref="adapted" />
  *  <see cref="itn" />
  *  <see cref="language_description" />
  *  <see cref="word_delimiter" />
  *  <see cref="writing_direction" />
  *  </summary>
  */
public class RecognitionStartedMessage_LanguagePackInfo
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="adapted">Whether the language pack is adapted with Language Model Adaptation.</param>
      *  <param name="itn">Whether Inverse Text Normalization (ITN) is available for this language.</param>
      *  <param name="language_description">The full name of the language.</param>
      *  <param name="word_delimiter">The character to put between words.</param>
      *  <param name="writing_direction"><c>left-to-right</c> or <c>right-to-left</c></param>
      *
      *  <see cref="adapted" />
      *  <see cref="itn" />
      *  <see cref="language_description" />
      *  <see cref="word_delimiter" />
      *  <see cref="writing_direction" />
      *  </summary>
      */
    public RecognitionStartedMessage_LanguagePackInfo (bool adapted, bool itn, string language_description,
        string word_delimiter, string writing_direction)
    {
        this.adapted = adapted;
        this.itn = itn;
        this.language_description = language_description;
        this.word_delimiter = word_delimiter;
        this.writing_direction = writing_direction;
    }

    /**
      *  <value>
      *  Whether the language pack is adapted with Language Model Adaptation (an upcoming feature).
      *  </value>
      */
    public bool adapted { get; set; }

    /**
      *  <value>
      *  Whether Inverse Text Normalization (ITN) is available for this language. ITN improves the formatting of
      *  entities in the text such as numerals and dates.
      *  </value>
      */
    public bool itn { get; set; }

    /**
      *  <value>
      *  The full name of the language.
      *  </value>
      */
    public string language_description { get; set; }

    /**
      *  <value>
      *  The character to put between words.
      *  </value>
      */
    public string word_delimiter { get; set; }

    /**
      *  <value>
      *  <c>left-to-right</c> or <c>right-to-left</c>
      *  </value>
      */
    public string writing_direction { get; set; }
}
