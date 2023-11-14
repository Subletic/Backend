using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Backend.Controllers;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

namespace BackendTests
{
    [TestFixture]
    public class CustomDictionaryServiceTests
    {
        private CustomDictionaryService _customDictionaryService;

        public CustomDictionaryServiceTests()
        {
            _customDictionaryService = new CustomDictionaryService();
        }

        [Test]
        public void ProcessCustomDictionary_AddsNewDictionary()
        {
            // Arrange
            var customDictionary = CreateSampleCustomDictionary("en", "SampleContent");

            // Act
            _customDictionaryService.ProcessCustomDictionary(customDictionary!);

            // Assert
            var dictionaries = _customDictionaryService.GetCustomDictionaries();
            Assert.That(dictionaries.Count, Is.EqualTo(1));
            Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.language, Is.EqualTo("en"));
            Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.additionalVocab?[0]?.Content, Is.EqualTo("SampleContent"));
            Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.additionalVocab?[0]?.SoundsLike?.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessCustomDictionary_UpdatesExistingDictionary()
        {
            // Arrange
            var customDictionary = CreateSampleCustomDictionary("en", "SampleContent");
            _customDictionaryService.ProcessCustomDictionary(customDictionary!);

            // Act
            // Update the existing dictionary with the same language and content
            customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab[0].Content = "UpdatedContent";
            customDictionary.StartRecognitionMessageTranscriptionConfig.additionalVocab[0].SoundsLike = new List<string> { "SimilarWord" };
            _customDictionaryService.ProcessCustomDictionary(customDictionary);

            // Assert
            var dictionaries = _customDictionaryService.GetCustomDictionaries();
            Assert.That(dictionaries.Count, Is.EqualTo(1));
            Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.additionalVocab?[0]?.Content, Is.EqualTo("UpdatedContent"));
            Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.additionalVocab?[0]?.SoundsLike?.Count, Is.EqualTo(1));
            Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.additionalVocab?[0]?.SoundsLike?[0], Is.EqualTo("SimilarWord"));
        }

        private static Dictionary CreateSampleCustomDictionary(string language, string content, List<string> soundsLike = null!)
        {
            var additionalVocab = new AdditionalVocab(content, soundsLike);
            var transcriptionConfig = new StartRecognitionMessage_TranscriptionConfig(language, false, new List<AdditionalVocab> { additionalVocab });
            return new Dictionary(transcriptionConfig);
        }
    }
}
