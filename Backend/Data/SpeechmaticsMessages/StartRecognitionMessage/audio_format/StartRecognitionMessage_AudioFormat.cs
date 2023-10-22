namespace Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;

/**
  *  <summary>
  *  Audio stream format that will be sent.
  *
  *  API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#supported-audio-types" />
  *
  *  <see cref="type" />
  *  <see cref="encoding" />
  *  <see cref="sample_rate" />
  *  </summary>
  */
public class StartRecognitionMessage_AudioFormat
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
    public StartRecognitionMessage_AudioFormat (
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
