using Backend.Data;
using System;
using System.Collections.Generic;
using Serilog;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

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
            if (customDictionary == null || customDictionary.StartRecognitionMessageTranscriptionConfig == null)
            {
                throw new ArgumentException("Invalid custom dictionary data.");
            }

            if (customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab.Count > 1000)
            {
                throw new ArgumentException("additionalVocab list cannot exceed 1000 elements.");
            }

            Log.Information($"Received custom dictionary for language {customDictionary.StartRecognitionMessageTranscriptionConfig.language}");

            var existingDictionary = _customDictionaries.FirstOrDefault(d =>
                d.StartRecognitionMessageTranscriptionConfig.additionalVocab.Any(av => av.Content == customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab.FirstOrDefault()?.Content)
            );

            if (existingDictionary != null)
            {
                existingDictionary.StartRecognitionMessageTranscriptionConfig = customDictionary.StartRecognitionMessageTranscriptionConfig;
                foreach (var av in existingDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab)
                {
                    av.SoundsLike = customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab[0].SoundsLike;
                }
                Log.Information($"Custom dictionary updated for content {customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab.FirstOrDefault()?.Content}");
            }
            else
            {
                _customDictionaries.Add(customDictionary);
                Log.Information($"Custom dictionary added to the in-memory data structure for content {customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab.FirstOrDefault()?.Content}");
            }
        }

        public List<Dictionary> GetCustomDictionaries()
        {
            return _customDictionaries;
        }
    }
}
