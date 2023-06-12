namespace Backend.Data;

public class SpeechmaticsStartRecognition
{
    public SpeechmaticsStartRecognition (
        SpeechmaticsStartRecognition_AudioType? audio_format = null,
        SpeechmaticsStartRecognition_TranscriptionConfig? transcription_config = null)
    {
        this.audio_format = (audio_format is not null)
            ? (SpeechmaticsStartRecognition_AudioType) audio_format
            : new SpeechmaticsStartRecognition_AudioType();
        this.transcription_config = (transcription_config is not null)
            ? (SpeechmaticsStartRecognition_TranscriptionConfig) transcription_config
            : new SpeechmaticsStartRecognition_TranscriptionConfig();
    }

    private static readonly string _message = "StartRecognition";

    // for JSON deserialising never actually let this be written, just verify
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
    public SpeechmaticsStartRecognition_AudioType audio_format { get; set; }
    public SpeechmaticsStartRecognition_TranscriptionConfig transcription_config { get; set; }
    // TODO add? irrelevant for our purposes
    // public SpeechmaticsStartRecognition_TranslationConfig? translation_config { get; set; }
}

public class SpeechmaticsStartRecognition_AudioType
{
    public SpeechmaticsStartRecognition_AudioType (
        string type = "raw",
        string? encoding = "pcm_s16le",
        int? sample_rate = 48000)
    {
        this.type = type;
        this.encoding = encoding;
        this.sample_rate = sample_rate;
    }

    private string? _type;
    private string? _encoding;
    private int? _sample_rate;

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

    public int getCheckedSampleRate() {
        if (!sample_rate.HasValue) throw new InvalidOperationException (
            "sample_rate is null");
        return (int)sample_rate;
    }

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

public class SpeechmaticsStartRecognition_TranscriptionConfig
{
    public SpeechmaticsStartRecognition_TranscriptionConfig (string language = "de", bool? enable_partials = false)
    {
        this.language = language;
        this.enable_partials = enable_partials;
    }

    // not sure how to validate that language is an ISO language code
    public string language { get; set; }
    public bool? enable_partials { get; set; }
}
