namespace Backend.Services;

using System;
using System.Collections.Generic;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Serilog;
using Serilog.Events;

/// <summary>
/// Dienst zur Verwaltung benutzerdefinierter Wörterbücher.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    // Das private readonly Feld logger wird verwendet, um den Logger für die Protokollierung innerhalb dieser Klasse zu halten.
    private readonly Serilog.ILogger logger;

    // List to store custom dictionaries.
    private List<StartRecognitionMessage_TranscriptionConfig> customDictionaries;

    // Variable, um zeitbasierte Wartezeiten für Funktionen im ConfigurationServiceController und BufferTimeMonitor zu speichern.
    private float delay;

    /// <summary>
    /// Konstruktor für den ConfigurationService. Initialisiert die Liste der benutzerdefinierten Wörterbücher.
    /// </summary>
    public ConfigurationService(Serilog.ILogger logger)
    {
        customDictionaries = new List<StartRecognitionMessage_TranscriptionConfig>();
        this.logger = logger;
    }

    /// <summary>
    /// Verarbeitet ein benutzerdefiniertes Wörterbuch.
    /// </summary>
    /// <param name="customDictionary">Das zu verarbeitende benutzerdefinierte Wörterbuch.</param>
    /// <exception cref="ArgumentException">Ausgelöst, wenn die Daten des benutzerdefinierten Wörterbuchs ungültig sind.</exception>
    public void ProcessCustomDictionary(StartRecognitionMessage_TranscriptionConfig customDictionary)
    {
        if (customDictionary == null)
        {
            throw new ArgumentException("Invalid custom dictionary data.");
        }

        // Check if the additionalVocab list exceeds the limit.
        if (customDictionary.additional_vocab.Count > 1000)
        {
            throw new ArgumentException("additionalVocab list cannot exceed 1000 elements.");
        }

        // Log information about the received custom dictionary.
        logger.Information($"Received custom dictionary for language {customDictionary.language}");

        // Find an existing dictionary with similar content.
        var existingDictionary = customDictionaries.FirstOrDefault(d =>
            d.additional_vocab.Any(av => av.content == customDictionary.additional_vocab.FirstOrDefault()?.content));

        // If an existing dictionary is found, update it; otherwise, add the new dictionary.
        if (existingDictionary == null)
        {
            customDictionaries.Add(customDictionary);
            logger.Information($"Custom dictionary added to the in-memory data structure for content {customDictionary.additional_vocab.FirstOrDefault()?.content}");
            return;
        }

        existingDictionary = customDictionary;
        foreach (var av in existingDictionary.additional_vocab)
        {
            av.sounds_like = customDictionary.additional_vocab[0].sounds_like;
        }

        logger.Information($"Custom dictionary updated for content {customDictionary.additional_vocab.FirstOrDefault()?.content}");
    }

    /// <summary>
    /// Gibt die Liste der benutzerdefinierten Wörterbücher zurück.
    /// </summary>
    /// <returns>Die Liste der benutzerdefinierten Wörterbücher.</returns>
    public List<StartRecognitionMessage_TranscriptionConfig> GetCustomDictionaries()
    {
        return customDictionaries;
    }

    /// <summary>
    /// Gibt den Wert der Verzögerung zurück.
    /// </summary>
    /// <returns>Die Verzögerung.</returns>
    public float GetDelay()
    {
        return this.delay;
    }

    /// <summary>
    /// Setzt den Wert der Verzögerung.
    /// </summary>
    /// <param name="delay">Die neue Verzögerung.</param>
    public void SetDelay(float delay)
    {
        this.delay = delay;
    }
}
