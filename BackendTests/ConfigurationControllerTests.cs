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
public class ConfigurationControllerTests
{
    private ConfigurationController customDictionaryController;
    private ConfigurationService customDictionaryService;
    private Serilog.ILogger logger;

    [SetUp]
    public void Setup()
    {
        logger = new LoggerConfiguration().CreateLogger();

        customDictionaryService = new ConfigurationService(logger);
        customDictionaryController = new ConfigurationController(customDictionaryService, logger);
    }

    [Test]
    public void UploadCustomDictionary_ValidData_ReturnsOk()
    {
        // Arrange
        var additionalVocab = new AdditionalVocab("word");
        var transcriptionConfig = new StartRecognitionMessage_TranscriptionConfig("de", true, new List<AdditionalVocab> { additionalVocab });
        var configurationData = new ConfigurationData(transcriptionConfig, 2.0f);

        // Act
        var result = customDictionaryController.UploadCustomDictionary(configurationData);

        // Logging
        if (result is BadRequestObjectResult badRequest)
        {
            logger.Information($"Request failed: {badRequest.Value}");
        }
        else
        {
            logger.Information("Request successful.");
            logger.Information($"Result Type: {result?.GetType().Name}");
            logger.Information($"Result Value: {result}");
        }

        // Assertions
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.IsFalse(result is BadRequestObjectResult, "Result should not be a BadRequest.");
        Assert.IsNotNull(((ObjectResult)result)?.StatusCode, "Status code should not be null.");
        Assert.That(((ObjectResult)result)?.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public void UploadCustomDictionary_InvalidData_ReturnsBadRequest()
    {
        // Act with invalid data (for example, passing null)
        var result = customDictionaryController.UploadCustomDictionary(null);

        // Assert
        Assert.IsNotNull(customDictionaryController, "Custom dictionary controller should not be null.");
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        Assert.IsNotNull(((BadRequestObjectResult)result)?.Value, "Result value should not be null.");
        Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Invalid custom dictionary data."));
    }
}
