using Backend.Data;
using System;
using System.Collections.Generic;
using Serilog;

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

            Log.Information($"Received custom dictionary for language {customDictionary.TranscriptionConfig.Language}");

            var existingDictionary = _customDictionaries.FirstOrDefault(d =>
                d.TranscriptionConfig.AdditionalVocab.Any(av => av.Content == customDictionary.TranscriptionConfig.AdditionalVocab.FirstOrDefault()?.Content)
            );

            if (existingDictionary != null)
            {
                existingDictionary.TranscriptionConfig = customDictionary.TranscriptionConfig;
                foreach (var av in existingDictionary.TranscriptionConfig.AdditionalVocab)
                {
                    av.SoundsLike = customDictionary.TranscriptionConfig.AdditionalVocab[0].SoundsLike;
                }
                Log.Information($"Custom dictionary updated for content {customDictionary.TranscriptionConfig.AdditionalVocab.FirstOrDefault()?.Content}");
            }
            else
            {
                _customDictionaries.Add(customDictionary);
                Log.Information($"Custom dictionary added to the in-memory data structure for content {customDictionary.TranscriptionConfig.AdditionalVocab.FirstOrDefault()?.Content}");
            }
        }

        public List<Dictionary> GetCustomDictionaries()
        {
            return _customDictionaries;
        }
    }
}
