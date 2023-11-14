#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Data.SpeechmaticsMessages.AddTranscriptMessage;

using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.metadata;
using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result;

/**
  * <summary>
  * Speechmatics RT API message: AddTranscript
  * Direction: Server -> Client
  * When: After several <c>AddAudio</c> message -> <c>AudioAddedMessage</c>
  * Purpose: Present the Client with the final transcription of some audio
  * Effects: Transcription data must be converted and put into the Backend for further processing and transmission
  * API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#addtranscript" />
  * <see cref="AudioAddedMessage" />
  * </summary>
  */
public class AddTranscriptMessage
{
    /**
      * <summary>
      * Simple constructor.
      * <see cref="format" />
      * <see cref="metadata" />
      * <see cref="results" />
      * <see cref="AddTranscriptMessage_Metadata" />
      * <see cref="AddTranscriptMessage_Result" />
      * </summary>
      * <param name="format">
      * Undocumented, maybe to identify when changes to the format have been made?
      * <c>2.9</c> at the time of writing.
      * </param>
      * <param name="metadata">Metadata about the entire transcript.</param>
      * <param name="results">Metadata about the transcript, element-by-element.</param>
      */
    public AddTranscriptMessage(string format, AddTranscriptMessage_Metadata metadata, List<AddTranscriptMessage_Result> results)
    {
        this.format = format;
        this.metadata = metadata;
        this.results = results;
    }

    /**
      * <summary>
      * The internal name of this message.
      * </summary>
      */
    private const string MESSAGE_TYPE = "AddTranscript";

    /**
      * <summary>
      * Gets or sets the message name.
      * Settable for JSON deserialising purposes, but value
      * *MUST* match <c>MESSAGE_TYPE</c> when attempting to set.
      * </summary>
      */
    public string message
    {
        get
        {
            return MESSAGE_TYPE;
        }

        set
        {
            if (value != MESSAGE_TYPE) throw new ArgumentException(String.Format("wrong message type: expected {0}, received {1}", MESSAGE_TYPE, value));
        }
    }

    /**
      * <summary>
      * Gets or sets the format.
      * Undocumented, maybe to identify when changes to the format have been made?
      * <c>2.9</c> at the time of writing.
      * </summary>
      */
    public string format { get; set; }

    /**
      * <summary>
      * Gets or sets the metadata about the entire transcript.
      * </summary>
      */
    public AddTranscriptMessage_Metadata metadata { get; set; }

    /**
      * <summary>
      * Gets or sets the metadata about the transcript, element-by-element.
      * </summary>
      */
    public List<AddTranscriptMessage_Result> results { get; set; }
}
