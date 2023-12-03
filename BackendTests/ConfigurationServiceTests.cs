using System;
using System.Collections.Generic;
using Backend.Controllers;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Serilog;

namespace BackendTests
{
    [TestFixture]
    public class ConfigurationServiceTests
    {
        private ConfigurationService? customDictionaryService;

        [SetUp]
        public void Setup()
        {
            var logger = new LoggerConfiguration().CreateLogger();
            customDictionaryService = new ConfigurationService(logger);
        }

        [Test]
        public void ProcessCustomDictionary_AddsNewDictionary()
        {
            // Arrange
            var customDictionary = createSampleCustomDictionary("de", "SampleContent");

            // Act
            customDictionaryService?.ProcessCustomDictionary(customDictionary);

            // Assert
            var dictionaries = customDictionaryService?.GetCustomDictionaries();
            Assert.That(dictionaries?.Count ?? 0, Is.EqualTo(1));
            Assert.That(dictionaries[0]?.language, Is.EqualTo("de"));
            Assert.That(dictionaries[0]?.additional_vocab?[0]?.content, Is.EqualTo("SampleContent"));
            Assert.That(dictionaries[0]?.additional_vocab?[0]?.sounds_like?.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessCustomDictionary_UpdatesExistingDictionary()
        {
            // Arrange
            var customDictionary = createSampleCustomDictionary("de", "SampleContent");
            customDictionaryService?.ProcessCustomDictionary(customDictionary);

            // Act
            // Update the existing dictionary with the same language and content
            customDictionary.additional_vocab[0].content = "UpdatedContent";
            customDictionary.additional_vocab[0].sounds_like = new List<string> { "SimilarWord" };
            customDictionaryService?.ProcessCustomDictionary(customDictionary);

            // Assert
            var dictionaries = customDictionaryService?.GetCustomDictionaries();
            Assert.That(dictionaries?.Count ?? 0, Is.EqualTo(1));
            Assert.That(dictionaries[0]?.additional_vocab?[0]?.content, Is.EqualTo("UpdatedContent"));
            Assert.That(dictionaries[0]?.additional_vocab?[0]?.sounds_like?.Count, Is.EqualTo(1));
            Assert.That(dictionaries[0]?.additional_vocab?[0]?.sounds_like?[0], Is.EqualTo("SimilarWord"));
        }

        private static StartRecognitionMessage_TranscriptionConfig createSampleCustomDictionary(string language, string content, List<string>? sounds_like = null)
        {
            List<AdditionalVocab> additionalVocab = new List<AdditionalVocab>();
            if (sounds_like != null)
            {
                foreach (var sound in sounds_like)
                {
                    additionalVocab.Add(new AdditionalVocab(content, new List<string> { sound }));
                }
            }
            else
            {
                additionalVocab.Add(new AdditionalVocab(content));
            }

            return new StartRecognitionMessage_TranscriptionConfig(language, false, additionalVocab);
        }
    }
}
