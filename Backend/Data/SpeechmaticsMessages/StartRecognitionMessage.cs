namespace Backend.Data.SpeechmaticsMessages;

/**
  *  <summary>
  *  Speechmatics RT API message: StartRecognition
  *
  *  Direction: Client -> Server
  *  When: After WebSocket connection has been opened
  *  Purpose: Signal that Server that we want to start a transcription process
  *  Effects: n/a, depends on <c>RecognitionStartedMessage</c> response
  *
  *  <see cref="RecognitionStartedMessage" />
  *  </summary>
  */
public class StartRecognitionMessage
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="audio_format">
      *  Audio stream type that will be sent.
      *  If null, sane defaults for our purposes will be used.
      *  </param>
      *  <param name="transcription_config">
      *  Configuration for this recognition session.
      *  If null, sane defaults for our purposes will be used.
      *  </param>
      *
      *  <see cref="audio_format" />
      *  <see cref="transcription_config" />
      *  <see cref="StartRecognitionMessage_AudioType" />
      *  <see cref="StartRecognitionMessage_TranscriptionConfig" />
      *  </summary>
      */
    public StartRecognitionMessage (
        StartRecognitionMessage_AudioType? audio_format = null,
        StartRecognitionMessage_TranscriptionConfig? transcription_config = null)
    {
        this.audio_format = (audio_format is not null)
            ? (StartRecognitionMessage_AudioType) audio_format
            : new StartRecognitionMessage_AudioType();
        this.transcription_config = (transcription_config is not null)
            ? (StartRecognitionMessage_TranscriptionConfig) transcription_config
            : new StartRecognitionMessage_TranscriptionConfig();
    }

    /**
      *  <summary>
      *  The internal name of this message.
      *  </summary>
      */
    private static readonly string _message = "StartRecognition";

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
      *  Audio stream type that will be sent.
      *  </value>
      */
    public StartRecognitionMessage_AudioType audio_format { get; set; }

    /**
      *  <value>
      *  Configuration for this recognition session.
      *  </value>
      */
    public StartRecognitionMessage_TranscriptionConfig transcription_config { get; set; }

    // TODO add? irrelevant for our purposes
    // public StartRecognitionMessage_TranslationConfig? translation_config { get; set; }
}

/**
  *  <summary>
  *  Audio stream type that will be sent.
  *
  *  <see cref="type" />
  *  <see cref="encoding" />
  *  <see cref="sample_rate" />
  *  </summary>
  */
public class StartRecognitionMessage_AudioType
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="type">The type of data that will be sent.</param>
      *  <param name="encoding">If <c>raw</c>, the sample encoding.</param>
      *  <param name="sample_rate">If <c>raw</c>, the sample rate.</param>
      *
      *  <see cref="type" />
      *  <see cref="encoding" />
      *  <see cref="sample_rate" />
      *  </summary>
      */
    public StartRecognitionMessage_AudioType (
        string type = "raw",
        string? encoding = "pcm_s16le",
        int? sample_rate = 48000)
    {
        this.type = type;
        this.encoding = encoding;
        this.sample_rate = sample_rate;
    }

    /**
      *  <value>
      *  Bare <c>type</c> value.
      *  </value>
      */
    private string? _type;

    /**
      *  <value>
      *  Bare <c>encoding</c> value.
      *  </value>
      */
    private string? _encoding;

    /**
      *  <value>
      *  Bare <c>sample_rate</c> value.
      *  </value>
      */
    private int? _sample_rate;

    /**
      *  <value>
      *  The type of data that will be sent.
      *
      *  Accepted values:
      *  - <c>raw</c>: raw audio data
      *  - <c>file</c>: a media file that GStreamer can digest, including header information
      *  </value>
      */
    public string type
    {
        get
        {
            if (_type is null) throw new InvalidOperationException (
                "null is not a valid state for type, assign a valid value first");
            return (string)_type;
        }

        set
        {
            switch (value)
            {
                case "raw":
                case "file":
                    _type = value;
                    break;
                default:
                    throw new ArgumentException (String.Format (
                        "{0} is not a valid type value", value), nameof (value));
            }
        }
    }

    /**
      *  <value>
      *  If <c>raw</c>, encoding used to store individual audio samples.
      *
      *  Accepted values:
      *  - <c>pcm_f32le</c>: 32-bit float PCM, little-endian
      *  - <c>pcm_s16le</c>: 16-bit signed integer PCM, little-endian
      *  - <c>mulaw</c>: 8-bit μ-law PCM
      *
      *  If <c>raw</c>, must be present.
      *  </value>
      */
    public string? encoding
    {
        get => _encoding;

        set
        {
            switch (value)
            {
                case "pcm_f32le":
                case "pcm_s16le":
                case "mulaw":
                case null:
                    _encoding = value;
                    break;
                default:
                    throw new ArgumentException (String.Format (
                        "{0} is not a valid encoding value", value), nameof (value));
            }
        }
    }

    /**
      *  <value>
      *  If <c>raw</c>, sample rate of the sent audio.
      *
      *  If <c>raw</c>, must be present.
      *  </value>
      */
    public int? sample_rate
    {
        get => _sample_rate;

        set
        {
            if (value.HasValue && value <= 0) throw new ArgumentOutOfRangeException (
                nameof (value), "Sample rate must be > 0");

            _sample_rate = value;
        }
    }

    /**
      *  <summary>
      *  Return <c>sample_rate</c> if set.
      *
      *  <returns><c>sample_rate</c></returns>
      *
      *  <exception cref="InvalidOperationException">If <c>sample_rate</c> is <c>null</c>.</exception>
      *  </summary>
      */
    public int getCheckedSampleRate() {
        if (!sample_rate.HasValue) throw new InvalidOperationException (
            "sample_rate is null");
        return (int)sample_rate;
    }

    /**
      *  <summary>
      *  Convert <c>encoding</c> into the corresponsing FFMpeg format argument if set.
      *
      *  <returns>A <c>string</c> representing a format FFMpeg should understand for format conversions</returns>
      *
      *  <exception cref="InvalidOperationException">If <c>encoding</c> is <c>null</c> or unknown.</exception>
      *  </summary>
      */
    public string encodingToFFMpegFormat()
    {
        if (type != "raw") throw new InvalidOperationException (String.Format (
            "don't know ffmpeg format string for type: {0}",
            type));

        switch (encoding)
        {
            case "pcm_f32le":
                return "f32le";
            case "pcm_s16le":
                return "s16le";
            case "mulaw":
                return "mulaw";
            default:
                throw new InvalidOperationException (String.Format (
                    "don't know ffmpeg format string for encoding: {0}",
                    encoding));
        }
    }

    /**
      *  <summary>
      *  Convert <c>encoding</c> into the amount of bytes per sample if set.
      *
      *  <returns>The number of bytes a single sample in that format requires</returns>
      *
      *  <exception cref="InvalidOperationException">If <c>encoding</c> is <c>null</c> or unknown.</exception>
      *  </summary>
      */
    public uint bytesPerSample()
    {
        if (type != "raw") throw new InvalidOperationException (String.Format (
            "don't know amount of bytes per sample for type: {0}",
            type));

        switch (encoding)
        {
            case "pcm_f32le":
                return 4;
            case "pcm_s16le":
                return 2;
            case "mulaw":
                return 1;
            default:
                throw new InvalidOperationException (String.Format (
                    "don't know amount of bytes per sample for encoding: {0}",
                    encoding));
        }
    }
}

/**
  *  <summary>
  *  Configuration for this recognition session.
  *
  *  <see cref="language" />
  *  <see cref="enable_partials" />
  *  </summary>
  */
public class StartRecognitionMessage_TranscriptionConfig
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="language">Language model to process audio, usually in ISO language code format.</param>
      *  <param name="enable_partials">Whether to send partial transcripts (<c>AddPartialTranscript</c> messages).</param>
      *
      *  <see cref="language" />
      *  <see cref="enable_partials" />
      *  </summary>
      */
    public StartRecognitionMessage_TranscriptionConfig (string language = "de", bool? enable_partials = false)
    {
        this.language = language;
        this.enable_partials = enable_partials;
    }

    // not sure how to validate that language is an ISO language code
    /**
      *  <value>
      *  Language model to process audio, usually in ISO language code format.
      *  The value must be consistent with the language code used in the API endpoint URL.
      *  </value>
      */
    public string language { get; set; }

    /**
      *  <value>
      *  Whether or not to send partials (i.e. `AddPartialTranscript` messages)
      *  in addition to the finals (i.e. `AddTranscriptMessage`s).
      *
      *  For now, the partials are somewhat useless to us. They are sent as a slowly accumulating list, each time
      *  resending the previous values together with the new ones. It might be possible to similarly accumulate values
      *  from partials in a special structure and only register the words that aren't resends.
      *  I don't know if there are any guarantees about the stability of transcriptions between partials. I think it's
      *  possible that the AI can go back and correct words in the partials, in which case these are wholly incompatible
      *  with our expectations. This could use some more testing.
      *  </value>
      */
    public bool? enable_partials { get; set; }
}
