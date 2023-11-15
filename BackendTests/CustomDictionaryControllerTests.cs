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
public class CustomDictionaryControllerTests
{
    private CustomDictionaryController? customDictionaryController;
    private CustomDictionaryService? customDictionaryService;

    [SetUp]
    public void Setup()
    {
        customDictionaryService = new CustomDictionaryService();
        customDictionaryController = new CustomDictionaryController(customDictionaryService!);
    }

    [Test]
    public void UploadCustomDictionary_ValidData_ReturnsOk()
    {
        var additionalVocab = new AdditionalVocab("word");
        var transcriptionConfig = new StartRecognitionMessage_TranscriptionConfig("en", false, new List<AdditionalVocab> { additionalVocab });
        var result = customDictionaryController!.UploadCustomDictionary(transcriptionConfig);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result).Value, Is.EqualTo("Custom dictionary uploaded successfully."));
    }

    [Test]
    public void UploadCustomDictionary_InvalidData_ReturnsBadRequest()
    {
        // Simuliere ungültige Daten
        var result = customDictionaryController!.UploadCustomDictionary(null!);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Invalid custom dictionary data."));
    }
}
