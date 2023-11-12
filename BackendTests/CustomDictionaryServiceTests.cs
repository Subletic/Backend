using Backend.Data;
using Backend.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace BackendTests
{
    [TestFixture]
    public class CustomDictionaryServiceTests
    {
        private CustomDictionaryService _customDictionaryService;

        [SetUp]
        public void Setup()
        {
            _customDictionaryService = new CustomDictionaryService();
        }

        [Test]
        public void ProcessCustomDictionary_AddsNewDictionary()
        {
            // Arrange
            var customDictionary = CreateSampleCustomDictionary("en", "SampleContent");

            // Act
            _customDictionaryService.ProcessCustomDictionary(customDictionary);

            // Assert
            var dictionaries = _customDictionaryService.GetCustomDictionaries();
            Assert.That(dictionaries.Count, Is.EqualTo(1));
            Assert.That(dictionaries[0]?.TranscriptionConfig?.Language, Is.EqualTo("en"));
            Assert.That(dictionaries[0]?.TranscriptionConfig?.AdditionalVocab?[0]?.Content, Is.EqualTo("SampleContent"));
            Assert.That(dictionaries[0]?.TranscriptionConfig?.AdditionalVocab?[0]?.SoundsLike?.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessCustomDictionary_UpdatesExistingDictionary()
        {
            // Arrange
            var customDictionary = CreateSampleCustomDictionary("en", "SampleContent");
            _customDictionaryService.ProcessCustomDictionary(customDictionary);

            // Act
            // Update the existing dictionary with the same language and content
            customDictionary.TranscriptionConfig.AdditionalVocab[0].Content = "UpdatedContent";
            customDictionary.TranscriptionConfig.AdditionalVocab[0].SoundsLike = new List<string> { "SimilarWord" };
            _customDictionaryService.ProcessCustomDictionary(customDictionary);

            // Assert
            var dictionaries = _customDictionaryService.GetCustomDictionaries();
            Assert.That(dictionaries.Count, Is.EqualTo(1));
            Assert.That(dictionaries[0]?.TranscriptionConfig?.AdditionalVocab?[0]?.Content, Is.EqualTo("UpdatedContent"));
            Assert.That(dictionaries[0]?.TranscriptionConfig?.AdditionalVocab?[0]?.SoundsLike?.Count, Is.EqualTo(1));
            Assert.That(dictionaries[0]?.TranscriptionConfig?.AdditionalVocab?[0]?.SoundsLike?[0], Is.EqualTo("SimilarWord"));
        }

        private Dictionary CreateSampleCustomDictionary(string language, string content, List<string> soundsLike = null)
        {
            var additionalVocab = new AdditionalVocab(content, soundsLike);
            var transcriptionConfig = new TranscriptionConfig(language, new List<AdditionalVocab> { additionalVocab });
            return new Dictionary(transcriptionConfig);
        }
    }
}
