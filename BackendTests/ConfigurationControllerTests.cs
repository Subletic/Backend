using System;
using System.Collections.Generic;
using Backend.Controllers;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace BackendTests
{
    [TestFixture]
    public class ConfigurationControllerTests
    {
        private ConfigurationController? customDictionaryController;
        private ConfigurationService? customDictionaryService;

        [SetUp]
        public void Setup()
        {
            customDictionaryService = new ConfigurationService();
            customDictionaryController = new ConfigurationController(customDictionaryService);
        }

        [Test]
        public void UploadCustomDictionary_ValidData_ReturnsOk()
        {
            // Arrange
            var additionalVocab = new AdditionalVocab("word");
            var transcriptionConfig = new StartRecognitionMessage_TranscriptionConfig("en", false, new List<AdditionalVocab> { additionalVocab });
            var frontendDictionary = new SoundslikeDictionary(transcriptionConfig);

            // Act
            var configurationData = new ConfigurationData(frontendDictionary, 2.0f);
            var result = customDictionaryController?.UploadCustomDictionary(configurationData);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.IsNotNull(((OkObjectResult)result)?.Value, "Result value should not be null.");
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo("Custom dictionary uploaded successfully."));
        }

        [Test]
        public void UploadCustomDictionary_InvalidData_ReturnsBadRequest()
        {
            // Act with invalid data (for example, passing null)
            var result = customDictionaryController?.UploadCustomDictionary(null);

            // Assert
            Assert.IsNotNull(customDictionaryController, "Custom dictionary controller should not be null.");
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.IsNotNull(((BadRequestObjectResult)result)?.Value, "Result value should not be null.");
            Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Invalid custom dictionary data."));
        }
    }
}
