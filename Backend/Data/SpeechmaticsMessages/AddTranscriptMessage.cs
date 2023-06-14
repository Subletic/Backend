namespace Backend.Data.SpeechmaticsMessages;

public class AddTranscriptMessage
{
    public AddTranscriptMessage (string format, AddTranscriptMessage_metadata metadata,
        List<AddTranscriptMessage_result> results)
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
    public AddTranscriptMessage_metadata metadata { get; set; }
    public List<AddTranscriptMessage_result> results { get; set; }
}

public class AddTranscriptMessage_result
{
    public AddTranscriptMessage_result (string type, double start_time, double end_time, bool? is_eos = null,
      string? attaches_to = null, List<AddTranscriptMessage_result_alternative>? alternatives = null)
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

    public List<AddTranscriptMessage_result_alternative>? alternatives { get; set; }
}

public class AddTranscriptMessage_result_alternative
{
    public AddTranscriptMessage_result_alternative (string content, double confidence, string? language = null,
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

public class AddTranscriptMessage_metadata
{
    public AddTranscriptMessage_metadata (string transcript, double start_time, double end_time)
    {
        this.transcript = transcript;
        this.start_time = start_time;
        this.end_time = end_time;
    }

    public string transcript { get; set; }
    public double start_time { get; set; }
    public double end_time { get; set; }
}
