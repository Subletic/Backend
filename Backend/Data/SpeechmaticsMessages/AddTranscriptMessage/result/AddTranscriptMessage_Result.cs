using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result.alternative;

namespace Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result;

/**
  *  <summary>
  *  Metadata about the transcript, element-by-element.
  *
  *  API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#addtranscript" />
  *  (no separate section, just included in its parent message's section)
  *
  *  <see cref="type" />
  *  <see cref="start_time" />
  *  <see cref="end_time" />
  *  <see cref="is_eos" />
  *  <see cref="attaches_to" />
  *  <see cref="alternatives" />
  *  </summary>
  */
public class AddTranscriptMessage_Result
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="type"><c>word</c> or <c>punctuation</c>.</param>
      *  <param name="start_time">The time (in seconds) of the audio corresponding to the beginning of the
      *  result.</param>
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
    public AddTranscriptMessage_Result (string type, double start_time, double end_time, bool? is_eos = null,
      string? attaches_to = null, List<AddTranscriptMessage_Result_Alternative>? alternatives = null)
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
    public List<AddTranscriptMessage_Result_Alternative>? alternatives { get; set; }
}
