namespace BackendTests;

using System;
using System.Collections.Generic;
using Backend.Controllers;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Serilog;

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
        var dictionary = customDictionaryService?.GetCustomDictionaries();
        Assert.IsNotNull(dictionary);
        Assert.That(dictionary!.language, Is.EqualTo("de"));

        var additionalVocab = dictionary.additional_vocab;
        Assert.IsNotNull(additionalVocab);
        Assert.That(additionalVocab!.Count, Is.EqualTo(1));

        var firstAdditionalVocab = additionalVocab[0];
        Assert.IsNotNull(firstAdditionalVocab);
        Assert.That(firstAdditionalVocab!.content, Is.EqualTo("SampleContent"));
        Assert.That(firstAdditionalVocab!.sounds_like?.Count ?? 0, Is.EqualTo(0));
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
        var dictionary = customDictionaryService?.GetCustomDictionaries();
        Assert.IsNotNull(dictionary);

        var firstAdditionalVocab = dictionary.additional_vocab?[0];
        Assert.IsNotNull(firstAdditionalVocab);
        Assert.That(firstAdditionalVocab!.content, Is.EqualTo("UpdatedContent"));
        Assert.That(firstAdditionalVocab!.sounds_like?.Count ?? 0, Is.EqualTo(1));
        Assert.That(firstAdditionalVocab!.sounds_like?[0], Is.EqualTo("SimilarWord"));
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
