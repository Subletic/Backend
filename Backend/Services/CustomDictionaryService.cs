using Backend.Data;
using System;
using System.Collections.Generic;
using Serilog;  // Stellen Sie sicher, dass der korrekte Namespace für Serilog verwendet wird

namespace Backend.Services
{
    public class CustomDictionaryService
    {
        private List<Dictionary> _customDictionaries;


        public CustomDictionaryService()
        {
            _customDictionaries = new List<Dictionary>();
        }

        public void ProcessCustomDictionary(Dictionary customDictionary)
        {
            if (customDictionary == null || customDictionary.TranscriptionConfig == null)
            {
                throw new ArgumentException("Invalid custom dictionary data.");
            }

            if (customDictionary.TranscriptionConfig.AdditionalVocab.Count > 1000)
            {
                throw new ArgumentException("additionalVocab list cannot exceed 1000 elements.");
            }

            // Hier können Sie die Logik zum Speichern des Wörterbuchs hinzufügen, die vom Frontend kommt
            Log.Information($"Received custom dictionary for language {customDictionary.TranscriptionConfig.Language}");

            // Fügen Sie das Wörterbuch zur Liste hinzu
            _customDictionaries.Add(customDictionary);

            // Loggen Sie den Erfolg des Hinzufügens zum Wörterbuch
            Log.Information($"Custom dictionary added to the in-memory data structure for language {customDictionary.TranscriptionConfig.Language}");
        }

        public List<Dictionary> GetCustomDictionaries()
        {
            return _customDictionaries;
        }
    }
}
