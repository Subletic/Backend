namespace Backend.Data;

public class SpeechmaticsAddTranscript
{
    public SpeechmaticsAddTranscript (string format, SpeechmaticsAddTranscript_metadata metadata,
        List<SpeechmaticsAddTranscript_result> results)
    {
        this.format = format;
        this.metadata = metadata;
        this.results = results;
    }

    private static readonly string _message = "AddTranscript";

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

    public string format { get; set; }
    public SpeechmaticsAddTranscript_metadata metadata { get; set; }
    public List<SpeechmaticsAddTranscript_result> results { get; set; }
}

public class SpeechmaticsAddTranscript_result
{
    public SpeechmaticsAddTranscript_result (string type, double start_time, double end_time, bool? is_eos = null,
      string? attaches_to = null, List<SpeechmaticsAddTranscript_result_alternative>? alternatives = null)
    {
        this.type = type;
        this.start_time = start_time;
        this.end_time = end_time;
        this.is_eos = is_eos;
        this.attaches_to = attaches_to;
        this.alternatives = alternatives;
    }

    public string type { get; set; }
    public double start_time { get; set; }
    public double end_time { get; set; }

    // type: punctuation
    public bool? is_eos { get; set; }
    public string? attaches_to { get; set; }

    public List<SpeechmaticsAddTranscript_result_alternative>? alternatives { get; set; }
}

public class SpeechmaticsAddTranscript_result_alternative
{
    public SpeechmaticsAddTranscript_result_alternative (string content, double confidence, string? language = null,
        string? speaker = null)
    {
        this.content = content;
        this.confidence = confidence;
        this.language = language;
        this.speaker = speaker;
    }

    public string content { get; set; }
    public double confidence { get; set; }

    public string? language { get; set; }
    public string? speaker { get; set; }
}

public class SpeechmaticsAddTranscript_metadata
{
    public SpeechmaticsAddTranscript_metadata (string transcript, double start_time, double end_time)
    {
        this.transcript = transcript;
        this.start_time = start_time;
        this.end_time = end_time;
    }

    public string transcript { get; set; }
    public double start_time { get; set; }
    public double end_time { get; set; }
}
