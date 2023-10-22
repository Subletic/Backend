using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result;
using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.metadata;

namespace Backend.Data.SpeechmaticsMessages.AddTranscriptMessage;

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
      *  <see cref="AddTranscriptMessage_Metadata" />
      *  <see cref="AddTranscriptMessage_Result" />
      *  </summary>
      */
    public AddTranscriptMessage (string format, AddTranscriptMessage_Metadata metadata,
        List<AddTranscriptMessage_Result> results)
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
    public AddTranscriptMessage_Metadata metadata { get; set; }

    /**
      *  <value>
      *  Metadata about the transcript, element-by-element.
      *  </value>
      */
    public List<AddTranscriptMessage_Result> results { get; set; }
}
