namespace BackendTests;

using System;
using System.Collections.Generic;
using Backend.Controllers;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

[TestFixture]
public class CustomDictionaryServiceTests
{
    private CustomDictionaryService customDictionaryService;

    public CustomDictionaryServiceTests()
    {
        customDictionaryService = new CustomDictionaryService();
    }

    [Test]
    public void ProcessCustomDictionary_AddsNewDictionary()
    {
        // Arrange
        var customDictionary = createSampleCustomDictionary("en", "SampleContent");

        // Act
        customDictionaryService.ProcessCustomDictionary(customDictionary!);

        // Assert
        var dictionaries = customDictionaryService.GetCustomDictionaries();
        Assert.That(dictionaries.Count, Is.EqualTo(1));
        Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.language, Is.EqualTo("en"));
        Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.additional_vocab?[0]?.content, Is.EqualTo("SampleContent"));
        Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.additional_vocab?[0]?.sounds_like?.Count, Is.EqualTo(0));
    }

    [Test]
    public void ProcessCustomDictionary_UpdatesExistingDictionary()
    {
        // Arrange
        var customDictionary = createSampleCustomDictionary("en", "SampleContent");
        customDictionaryService.ProcessCustomDictionary(customDictionary!);

        // Act
        // Update the existing dictionary with the same language and content
        customDictionary.StartRecognitionMessageTranscriptionConfig.additional_vocab[0].content = "UpdatedContent";
        customDictionary.StartRecognitionMessageTranscriptionConfig.additional_vocab[0].sounds_like = new List<string> { "SimilarWord" };
        customDictionaryService.ProcessCustomDictionary(customDictionary);

        // Assert
        var dictionaries = customDictionaryService.GetCustomDictionaries();
        Assert.That(dictionaries.Count, Is.EqualTo(1));
        Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.additional_vocab?[0]?.content, Is.EqualTo("UpdatedContent"));
        Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.additional_vocab?[0]?.sounds_like?.Count, Is.EqualTo(1));
        Assert.That(dictionaries[0]?.StartRecognitionMessageTranscriptionConfig?.additional_vocab?[0]?.sounds_like?[0], Is.EqualTo("SimilarWord"));
    }

    private static Dictionary createSampleCustomDictionary(string language, string content, List<string> sounds_like = null!)
    {
        var additionalVocab = new additionalVocab(content, sounds_like);
        var transcriptionConfig = new StartRecognitionMessage_TranscriptionConfig(language, false, new List<additionalVocab> { additionalVocab });
        return new Dictionary(transcriptionConfig);
    }
}
