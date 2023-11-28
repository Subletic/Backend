namespace Backend.Data;

using System;
using System.Collections.Generic;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

/// <summary>
/// Class for the frondend dictionary.
/// </summary>
public class FrondendDictionary
{
    /// <summary>
    /// Gets or sets the TranscriptionConfig of the StartRecognitionMessage.
    /// </summary>
    public StartRecognitionMessage_TranscriptionConfig transcription_config { get; set; }

    /// <summary>
    /// Constructor of the frondend Dictionary class.
    /// </summary>
    /// <param name="startRecognitionMessageTranscriptionConfig">The TranscriptionConfig of the StartRecognitionMessage.</param>
    public FrondendDictionary(StartRecognitionMessage_TranscriptionConfig transcription_config)
    {
        this.transcription_config = transcription_config;
    }
}
