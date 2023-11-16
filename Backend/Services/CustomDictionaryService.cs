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

    /// <summary>
    /// Konstruktor für den CustomDictionaryService. Initialisiert die Liste der benutzerdefinierten Wörterbücher.
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
        if (customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab.Count > 1000)
        {
            throw new ArgumentException("additionalVocab list cannot exceed 1000 elements.");
        }

        // Log information about the received custom dictionary.
        Log.Information($"Received custom dictionary for language {customDictionary.StartRecognitionMessageTranscriptionConfig.language}");

        // Find an existing dictionary with similar content.
        var existingDictionary = customDictionaries.FirstOrDefault(d =>
            d.StartRecognitionMessageTranscriptionConfig.additionalVocab.Any(av => av.content == customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab.FirstOrDefault()?.content));

        // If an existing dictionary is found, update it; otherwise, add the new dictionary.
        if (existingDictionary != null)
        {
            existingDictionary.StartRecognitionMessageTranscriptionConfig = customDictionary.StartRecognitionMessageTranscriptionConfig;
            foreach (var av in existingDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab)
            {
                av.sounds_like = customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab[0].sounds_like;
            }

            Log.Information($"Custom dictionary updated for content {customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab.FirstOrDefault()?.content}");
        }
        else
        {
            customDictionaries.Add(customDictionary);
            Log.Information($"Custom dictionary added to the in-memory data structure for content {customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab.FirstOrDefault()?.content}");
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
}
