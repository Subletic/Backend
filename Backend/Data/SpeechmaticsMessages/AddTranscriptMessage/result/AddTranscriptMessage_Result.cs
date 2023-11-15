#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result;

using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result.alternative;

/// <summary>
/// Metadata about the transcript, element-by-element.
/// API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#addtranscript" />
/// (no separate section, just included in its parent message's section)
/// <see cref="type" />
/// <see cref="start_time" />
/// <see cref="end_time" />
/// <see cref="is_eos" />
/// <see cref="attaches_to" />
/// <see cref="alternatives" />
/// </summary>
public class AddTranscriptMessage_Result
{
    /// <summary>
    /// Simple constructor.
    /// <see cref="type" />
    /// <see cref="start_time" />
    /// <see cref="end_time" />
    /// <see cref="is_eos" />
    /// <see cref="attaches_to" />
    /// <see cref="alternatives" />
    /// <see cref="AddTranscriptMessage_result_alternative" />
    /// </summary>
    /// <param name="type"><c>word</c> or <c>punctuation</c>.</param>
    /// <param name="start_time">The time (in seconds) of the audio corresponding to the beginning of the
    /// result.</param>
    /// <param name="end_time">The time (in seconds) of the audio corresponding to the end of the result.</param>
    /// <param name="is_eos">If <c>punctuation</c>, whether mark is end-of-sentence symbol.</param>
    /// <param name="attaches_to">Undocumented. If <c>punctuation</c>, <c>previous</c>.</param>
    /// <param name="alternatives">List of possible words/symbols.</param>
    public AddTranscriptMessage_Result(
        string type,
        double start_time,
        double end_time,
        bool? is_eos = null,
        string? attaches_to = null,
        List<AddTranscriptMessage_Result_Alternative>? alternatives = null)
    {
        this.type = type;
        this.start_time = start_time;
        this.end_time = end_time;
        this.is_eos = is_eos;
        this.attaches_to = attaches_to;
        this.alternatives = alternatives;
    }

    /// <summary>
    /// Gets or sets the type of the result.
    /// One of <c>word</c> or <c>punctuation</c>.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// Gets or sets the time (in seconds) of the audio corresponding to the beginning of the result.
    /// </summary>
    public double start_time { get; set; }

    /// <summary>
    /// Gets or sets the time (in seconds) of the audio corresponding to the end of the result.
    /// <c>punctuation</c> are considered zero duration: <c>start_time == end_time</c>
    /// </summary>
    public double end_time { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether, if <c>punctuation</c>, the mark is an end-of-sentence symbol.
    /// For example, full-stops are an end-of-sentence symbol in English, whereas commas are not.
    /// Other languages may use different symbols and rules.
    /// </summary>
    public bool? is_eos { get; set; }

    /// <summary>
    /// Gets or sets attaches_to.
    /// Present if <c>punctuation</c>, presumable indicates which word the mark attaches to.
    /// <c>previous</c> in our tests.
    /// </summary>
    public string? attaches_to { get; set; }

    /// <summary>
    /// Gets or sets alternatives.
    /// List of possible words/symbols.
    /// Documentation says this is an optional (so making it <c>Nullable</c>),
    /// but also says that for all declared-possible values in <c>type</c>, it shall have at least 1 list entry.
    /// </summary>
    public List<AddTranscriptMessage_Result_Alternative>? alternatives { get; set; }
}
