namespace Backend.Services;

using System;
using System.Collections.Generic;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Serilog;

/// <summary>
/// Service for managing custom dictionaries.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    // Logger for logging within this class
    private readonly ILogger log;

    // Storage for custom dictionaries
    private StartRecognitionMessage_TranscriptionConfig? customDictionary;

    // Variable for storing time-based delays
    private float delay;

    /// <summary>
    /// Constructor for the ConfigurationService. Initializes the custom dictionaries list.
    /// </summary>
    /// <param name="log">The logger used for logging within this class.</param>
    public ConfigurationService(ILogger log)
    {
        this.log = log;
        this.customDictionary = null;
    }

    /// <summary>
    /// Processes a custom dictionary.
    /// </summary>
    /// <param name="newCustomDictionary">The custom dictionary to be processed.</param>
    /// <exception cref="ArgumentException">Thrown when the custom dictionary data is invalid.</exception>
    public void ProcessCustomDictionary(StartRecognitionMessage_TranscriptionConfig newCustomDictionary)
    {
        // Check if the new custom dictionary is null
        if (newCustomDictionary == null)
        {
            throw new ArgumentException("Invalid custom dictionary data.");
        }

        // Check if the number of additional vocab exceeds the maximum count
        if (newCustomDictionary.additional_vocab.Count > StartRecognitionMessage_TranscriptionConfig.MAX_ADDITIONAL_VOCAB_COUNT)
        {
            throw new ArgumentException($"additionalVocab list cannot exceed {StartRecognitionMessage_TranscriptionConfig.MAX_ADDITIONAL_VOCAB_COUNT} elements.");
        }

        // Log information about the received custom dictionary
        log.Information($"Received custom dictionary for language {customDictionary?.language}");

        // Check if the custom dictionary is null and add or update it
        if (customDictionary == null)
        {
            customDictionary = newCustomDictionary;
            log.Information($"Custom dictionary added to the in-memory data structure for content {customDictionary.additional_vocab.FirstOrDefault()?.content}");
            return;
        }

        customDictionary = newCustomDictionary;
        log.Information($"Custom dictionary updated for content {customDictionary.additional_vocab.FirstOrDefault()?.content}");
    }

    /// <summary>
    /// Returns the list of custom dictionaries.
    /// </summary>
    /// <returns>The custom dictionary.</returns>
    public StartRecognitionMessage_TranscriptionConfig? GetCustomDictionary()
    {
        return customDictionary;
    }

    /// <summary>
    /// Returns the value of the delay.
    /// </summary>
    /// <returns>The delay.</returns>
    public float GetDelay()
    {
        return this.delay;
    }

    /// <summary>
    /// Sets the value of the delay.
    /// </summary>
    /// <param name="delay">The new delay.</param>
    public void SetDelay(float delay)
    {
        this.delay = delay;
    }
}
