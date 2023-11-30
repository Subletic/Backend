namespace Backend.Data;

using System;
using System.Collections.Generic;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

/// <summary>
/// Represents configuration data for dictionaries with associated delay length.
/// </summary>
public class ConfigurationData
{
    /// <summary>
    /// Gets or sets the dictionary configuration data.
    /// </summary>
    public Dictionary dictionary { get; set; }

    /// <summary>
    /// Gets or sets the delay length associated with the configuration data.
    /// </summary>
    public float delayLength { get; set; }

    /// <summary>
    /// Initializes a new instance of the ConfigurationData class with dictionary and delay length.
    /// </summary>
    /// <param name="dictionary">The dictionary configuration.</param>
    /// <param name="delayLength">The delay length associated with the configuration.</param>
    public ConfigurationData(Dictionary dictionary, float delayLength)
    {
        this.dictionary = dictionary;
        this.delayLength = delayLength;
    }
}


