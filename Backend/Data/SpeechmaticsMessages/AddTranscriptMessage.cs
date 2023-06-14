namespace Backend.Data.SpeechmaticsMessages;

/**
  *  <summary>
  *  Speechmatics RT API message: AddTranscript
  *
  *  Direction: Server -> Client
  *  When: After several <c>AddAudio</c> message -> <c>AudioAddedMessage</c>
  *  Purpose: Present the Client with the final transcription of some audio
  *  Effects: Transcription data must be converted and put into the Backend for further processing and transmission
  *
  *  <see cref="AudioAddedMessage" />
  *  </summary>
  */
public class AddTranscriptMessage
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="format">
      *  Undocumented, maybe to identify when changes to the format have been made?
      *  <c>2.9</c> at the time of writing.
      *  </param>
      *  <param name="metadata">Metadata about the entire transcript.</param>
      *  <param name="results">Metadata about the transcript, element-by-element.</param>
      *
      *  <see cref="format" />
      *  <see cref="metadata" />
      *  <see cref="results" />
      *  <see cref="AddTranscriptMessage_metadata" />
      *  <see cref="AddTranscriptMessage_result" />
      *  </summary>
      */
    public AddTranscriptMessage (string format, AddTranscriptMessage_metadata metadata,
        List<AddTranscriptMessage_result> results)
    {
        this.format = format;
        this.metadata = metadata;
        this.results = results;
    }

    /**
      *  <summary>
      *  The internal name of this message.
      *  </summary>
      */
    private static readonly string _message = "AddTranscript";

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
      *  Undocumented, maybe to identify when changes to the format have been made?
      *  <c>2.9</c> at the time of writing.
      *  </value>
      */
    public string format { get; set; }

    /**
      *  <value>
      *  Metadata about the entire transcript.
      *  </value>
      */
    public AddTranscriptMessage_metadata metadata { get; set; }

    /**
      *  <value>
      *  Metadata about the transcript, element-by-element.
      *  </value>
      */
    public List<AddTranscriptMessage_result> results { get; set; }
}

/**
  *  <summary>
  *  Metadata about the transcript, element-by-element.
  *
  *  <see cref="type" />
  *  <see cref="start_time" />
  *  <see cref="end_time" />
  *  <see cref="is_eos" />
  *  <see cref="attaches_to" />
  *  <see cref="alternatives" />
  *  </summary>
  */
public class AddTranscriptMessage_result
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="type"><c>word</c> or <c>punctuation</c>.</param>
      *  <param name="start_time">The time (in seconds) of the audio corresponding to the beginning of the result.</param>
      *  <param name="end_time">The time (in seconds) of the audio corresponding to the end of the result.</param>
      *  <param name="is_eos">If <c>punctuation</c>, whether mark is end-of-sentence symbol.</param>
      *  <param name="attaches_to">Undocumented. If <c>punctuation</c>, <c>previous</c>.</param>
      *  <param name="alternatives">List of possible words/symbols.</param>
      *
      *  <see cref="type" />
      *  <see cref="start_time" />
      *  <see cref="end_time" />
      *  <see cref="is_eos" />
      *  <see cref="attaches_to" />
      *  <see cref="alternatives" />
      *  <see cref="AddTranscriptMessage_result_alternative" />
      *  </summary>
      */
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

    /**
      *  <value>
      *  Indicates what was detected.
      *  One of <c>word</c> or <c>punctuation</c>.
      *  </value>
      */
    public string type { get; set; }

    /**
      *  <value>
      *  The time (in seconds) of the audio corresponding to the beginning of the result.
      *  </value>
      */
    public double start_time { get; set; }

    /**
      *  <value>
      *  The time (in seconds) of the audio corresponding to the end of the result.
      *  <c>punctuation</c> are considered zero duration: <c>start_time == end_time</c>
      *  </value>
      */
    public double end_time { get; set; }

    /**
      *  <value>
      *  If <c>punctuation</c>, whether the mark is considered an end-of-sentence symbol.
      *  For example, full-stops are an end-of-sentence symbol in English, whereas commas are not.
      *  Other languages may use different symbols and rules.
      *  </value>
      */
    public bool? is_eos { get; set; }

    /**
      *  <value>
      *  Undocumented.
      *  Present if <c>punctuation</c>, presumable indicates which word the mark attaches to.
      *  <c>previous</c> in our tests.
      *  </value>
      */
    public string? attaches_to { get; set; }

    /**
      *  <value>
      *  List of possible words/symbols.
      *
      *  Documentation says this is an optional (so making it <c>Nullable</c>),
      *  but also says that for all declared-possible values in <c>type</c>, it shall have at least 1 list entry.
      *  </value>
      */
    public List<AddTranscriptMessage_result_alternative>? alternatives { get; set; }
}

/**
  *  <summary>
  *  A possible word/symbol.
  *
  *  <see cref="content" />
  *  <see cref="confidence" />
  *  <see cref="language" />
  *  <see cref="speaker" />
  *  </summary>
  */
public class AddTranscriptMessage_result_alternative
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="content">A word or punctuation mark.</param>
      *  <param name="confidence">A confidence score assigned to the alternative, from 0.0 to 1.0 (worst to best).</param>
      *  <param name="language">The language that the alternative word is assumed to be spoken in.</param>
      *  <param name="speaker">Diarisation label indicating who said that word.</param>
      *
      *  <see cref="content" />
      *  <see cref="confidence" />
      *  <see cref="language" />
      *  <see cref="speaker" />
      *  </summary>
      */
    public AddTranscriptMessage_result_alternative (string content, double confidence, string? language = null,
        string? speaker = null)
    {
        this.content = content;
        this.confidence = confidence;
        this.language = language;
        this.speaker = speaker;
    }

    /**
      *  <summary>
      *  A word or punctuation mark.
      *  </summary>
      */
    public string content { get; set; }

    /**
      *  <summary>
      *  A confidence score assigned to the alternative.
      *  Ranges from 0.0 (least confident) to 1.0 (most confident).
      *  </summary>
      */
    public double confidence { get; set; }

    /**
      *  <summary>
      *  The language that the alternative word is assumed to be spoken in.
      *  Currently, this will always be equal to the language that was requested
      *  in the initial <c>StartRecognitionMessage</c>.
      *  </summary>
      */
    public string? language { get; set; }

    /**
      *  <summary>
      *  Label indicating who said that word. Only set if diarization is enabled.
      *  </summary>
      */
    public string? speaker { get; set; }
}

/**
  *  <summary>
  *  Metadata about the entire transcript.
  *
  *  <see cref="transcript" />
  *  <see cref="start_time" />
  *  <see cref="end_time" />
  *  </summary>
  */
public class AddTranscriptMessage_metadata
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="transcript">The entire transcript contained in the segment.</param>
      *  <param name="start_time">The time (in seconds) of the audio corresponding to the beginning of the first word.</param>
      *  <param name="end_time">The time (in seconds) of the audio corresponding to the ending of the final word.</param>
      *
      *  <see cref="transcript" />
      *  <see cref="start_time" />
      *  <see cref="end_time" />
      *  </summary>
      */
    public AddTranscriptMessage_metadata (string transcript, double start_time, double end_time)
    {
        this.transcript = transcript;
        this.start_time = start_time;
        this.end_time = end_time;
    }

    /**
      *  <summary>
      *  The entire transcript contained in the segment, in plaintext (without confidences, timings, etc).
      *  For ease of consumption.
      *  </summary>
      */
    public string transcript { get; set; }

    /**
      *  <summary>
      *  The time (in seconds) of the audio corresponding to the beginning of the first word in the segment.
      *  </summary>
      */
    public double start_time { get; set; }

    /**
      *  <summary>
      *  The time (in seconds) of the audio corresponding to the ending of the final word in the segment.
      *  </summary>
      */
    public double end_time { get; set; }
}
