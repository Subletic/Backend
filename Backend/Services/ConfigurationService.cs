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
    // List to store custom dictionaries.
    private List<Dictionary> customDictionaries;

    /// Variable, um zeitbasierte Wartezeiten für Funktionen im ConfigurationServiceController und BufferTimeMonitor zu speichern.
    private float Delay;

    // Das private readonly Feld logger wird verwendet, um den Logger für die Protokollierung innerhalb dieser Klasse zu halten.
    private readonly Serilog.ILogger logger;

    /// <summary>
    /// Konstruktor für den ConfigurationService. Initialisiert die Liste der benutzerdefinierten Wörterbücher.
    /// </summary>
    public ConfigurationService(Serilog.ILogger logger)
    {
        customDictionaries = new List<Dictionary>();
        this.logger = logger;
    }

    /// <summary>
    /// Verarbeitet ein benutzerdefiniertes Wörterbuch.
    /// </summary>
    /// <param name="customDictionary">Das zu verarbeitende benutzerdefinierte Wörterbuch.</param>
    /// <exception cref="ArgumentException">Ausgelöst, wenn die Daten des benutzerdefinierten Wörterbuchs ungültig sind.</exception>
    public void ProcessCustomDictionary(Dictionary customDictionary)
    {
        if (customDictionary == null || customDictionary.transcription_config == null)
        {
            throw new ArgumentException("Invalid custom dictionary data.");
        }

        // Check if the additionalVocab list exceeds the limit.
        if (customDictionary.transcription_config.additional_vocab.Count > 1000)
        {
            throw new ArgumentException("additionalVocab list cannot exceed 1000 elements.");
        }

        // Log information about the received custom dictionary.
        logger.Information($"Received custom dictionary for language {customDictionary.transcription_config.language}");

        // Find an existing dictionary with similar content.
        var existingDictionary = customDictionaries.FirstOrDefault(d =>
            d.transcription_config.additional_vocab.Any(av => av.content == customDictionary.transcription_config.additional_vocab.FirstOrDefault()?.content));

        // If an existing dictionary is found, update it; otherwise, add the new dictionary.
        if (existingDictionary != null)
        {
            existingDictionary.transcription_config = customDictionary.transcription_config;
            foreach (var av in existingDictionary.transcription_config.additional_vocab)
            {
                av.sounds_like = customDictionary.transcription_config.additional_vocab[0].sounds_like;
            }

            logger.Information($"Custom dictionary updated for content {customDictionary.transcription_config.additional_vocab.FirstOrDefault()?.content}");
        }
        else
        {
            customDictionaries.Add(customDictionary);
            logger.Information($"Custom dictionary added to the in-memory data structure for content {customDictionary.transcription_config.additional_vocab.FirstOrDefault()?.content}");
        }
    }

    /// <summary>
    /// Gibt die Liste der benutzerdefinierten Wörterbücher zurück.
    /// </summary>
    /// <returns>Die Liste der benutzerdefinierten Wörterbücher.</returns>
    public List<Dictionary> GetCustomDictionaries()
    {
        return customDictionaries;
    }

    /// <summary>
    /// Gibt den Wert der Verzögerung zurück.
    /// </summary>
    /// <returns>Die Verzögerung.</returns>
    public float GetDelay()
    {
        return this.Delay;
    }

    /// <summary>
    /// Setzt den Wert der Verzögerung.
    /// </summary>
    /// <param name="delay">Die neue Verzögerung.</param>
    public void SetDelay(float delay)
    {
        this.Delay = delay;
    }
}
