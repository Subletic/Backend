namespace Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.metadata;

/**
  *  <summary>
  *  Metadata about the entire transcript.
  *
  *  <see cref="transcript" />
  *  <see cref="start_time" />
  *  <see cref="end_time" />
  *  </summary>
  */
public class AddTranscriptMessage_Metadata
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="transcript">The entire transcript contained in the segment.</param>
      *  <param name="start_time">The time (in seconds) of the audio corresponding to the beginning of the first
      *  word.</param>
      *  <param name="end_time">The time (in seconds) of the audio corresponding to the ending of the final
      *  word.</param>
      *
      *  <see cref="transcript" />
      *  <see cref="start_time" />
      *  <see cref="end_time" />
      *  </summary>
      */
    public AddTranscriptMessage_Metadata (string transcript, double start_time, double end_time)
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
