namespace Backend.Data;

using System;
using System.Collections.Generic;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

/// <summary>
/// Class for the custom dictionary.
/// </summary>
public class Dictionary
{
    /// <summary>
    /// Gets or sets the TranscriptionConfig of the StartRecognitionMessage.
    /// </summary>
    public StartRecognitionMessage_TranscriptionConfig StartRecognitionMessageTranscriptionConfig { get; set; }

    /// <summary>
    /// Constructor of the Dictionary class.
    /// </summary>
    /// <param name="startRecognitionMessageTranscriptionConfig">The TranscriptionConfig of the StartRecognitionMessage.</param>
    public Dictionary(StartRecognitionMessage_TranscriptionConfig startRecognitionMessageTranscriptionConfig)
    {
        StartRecognitionMessageTranscriptionConfig = startRecognitionMessageTranscriptionConfig;
    }
}
