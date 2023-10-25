using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

namespace Backend.Data.SpeechmaticsMessages.StartRecognitionMessage;

/**
  *  <summary>
  *  Speechmatics RT API message: StartRecognition
  *
  *  Direction: Client -> Server
  *  When: After WebSocket connection has been opened
  *  Purpose: Signal that Server that we want to start a transcription process
  *  Effects: n/a, depends on <c>RecognitionStartedMessage</c> response
  *  API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#startrecognition" />
  *
  *  <see cref="RecognitionStartedMessage" />
  *  </summary>
  */
public class StartRecognitionMessage
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="audio_format">
      *  Audio stream type that will be sent.
      *  If null, sane defaults for our purposes will be used.
      *  </param>
      *  <param name="transcription_config">
      *  Configuration for this recognition session.
      *  If null, sane defaults for our purposes will be used.
      *  </param>
      *
      *  <see cref="audio_format" />
      *  <see cref="transcription_config" />
      *  <see cref="StartRecognitionMessage_AudioFormat" />
      *  <see cref="StartRecognitionMessage_TranscriptionConfig" />
      *  </summary>
      */
    public StartRecognitionMessage (
        StartRecognitionMessage_AudioFormat? audio_format = null,
        StartRecognitionMessage_TranscriptionConfig? transcription_config = null)
    {
        this.audio_format = (audio_format is not null)
            ? (StartRecognitionMessage_AudioFormat) audio_format
            : new StartRecognitionMessage_AudioFormat();
        this.transcription_config = (transcription_config is not null)
            ? (StartRecognitionMessage_TranscriptionConfig) transcription_config
            : new StartRecognitionMessage_TranscriptionConfig();
    }

    /**
      *  <summary>
      *  The internal name of this message.
      *  </summary>
      */
    private const string MESSAGE_TYPE = "StartRecognition";

    /**
      *  <value>
      *  Message name.
      *  Settable for JSON deserialising purposes, but value
      *  *MUST* match <c>MESSAGE_TYPE</c> when attempting to set.
      *  </value>
      */
    public string message
    {
        get { return MESSAGE_TYPE; }
        set
        {
            if (value != MESSAGE_TYPE)
                throw new ArgumentException (String.Format (
                    "wrong message type: expected {0}, received {1}",
                    MESSAGE_TYPE, value));
        }
    }

    /**
      *  <value>
      *  Audio stream type that will be sent.
      *  </value>
      */
    public StartRecognitionMessage_AudioFormat audio_format { get; set; }

    /**
      *  <value>
      *  Configuration for this recognition session.
      *  </value>
      */
    public StartRecognitionMessage_TranscriptionConfig transcription_config { get; set; }

    // TODO add? irrelevant for our purposes
    // public StartRecognitionMessage_TranslationConfig? translation_config { get; set; }
}
