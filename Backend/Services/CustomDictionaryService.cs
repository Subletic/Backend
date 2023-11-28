namespace Backend.Services;

using System;
using System.Collections.Generic;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Serilog;

/// <summary>
/// Dienst zur Verwaltung benutzerdefinierter Wörterbücher.
/// </summary>
public class CustomDictionaryService : ICustomDictionaryService
{
    // List to store custom dictionaries.
    private List<Dictionary> customDictionaries;

    private float Delay;

    /// <summary>
    /// Konstruktor für den CustomDict  ionaryService. Initialisiert die Liste der benutzerdefinierten Wörterbücher.
    /// </summary>
    public CustomDictionaryService()
    {
        customDictionaries = new List<Dictionary>();
    }

    /// <summary>
    /// Verarbeitet ein benutzerdefiniertes Wörterbuch.
    /// </summary>
    /// <param name="customDictionary">Das zu verarbeitende benutzerdefinierte Wörterbuch.</param>
    /// <exception cref="ArgumentException">Ausgelöst, wenn die Daten des benutzerdefinierten Wörterbuchs ungültig sind.</exception>
    public void ProcessCustomDictionary(Dictionary customDictionary)
    {
        if (customDictionary == null || customDictionary.StartRecognitionMessageTranscriptionConfig == null)
        {
            throw new ArgumentException("Invalid custom dictionary data.");
        }

        // Check if the additionalVocab list exceeds the limit.
        if (customDictionary.StartRecognitionMessageTranscriptionConfig.additional_vocab.Count > 1000)
        {
            throw new ArgumentException("additionalVocab list cannot exceed 1000 elements.");
        }

        // Log information about the received custom dictionary.
        Log.Information($"Received custom dictionary for language {customDictionary.StartRecognitionMessageTranscriptionConfig.language}");

        // Find an existing dictionary with similar content.
        var existingDictionary = customDictionaries.FirstOrDefault(d =>
            d.StartRecognitionMessageTranscriptionConfig.additional_vocab.Any(av => av.content == customDictionary.StartRecognitionMessageTranscriptionConfig.additional_vocab.FirstOrDefault()?.content));

        // If an existing dictionary is found, update it; otherwise, add the new dictionary.
        if (existingDictionary != null)
        {
            existingDictionary.StartRecognitionMessageTranscriptionConfig = customDictionary.StartRecognitionMessageTranscriptionConfig;
            foreach (var av in existingDictionary.StartRecognitionMessageTranscriptionConfig.additional_vocab)
            {
                av.sounds_like = customDictionary.StartRecognitionMessageTranscriptionConfig.additional_vocab[0].sounds_like;
            }

            Log.Information($"Custom dictionary updated for content {customDictionary.StartRecognitionMessageTranscriptionConfig.additional_vocab.FirstOrDefault()?.content}");
        }
        else
        {
            customDictionaries.Add(customDictionary);
            Log.Information($"Custom dictionary added to the in-memory data structure for content {customDictionary.StartRecognitionMessageTranscriptionConfig.additional_vocab.FirstOrDefault()?.content}");
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
