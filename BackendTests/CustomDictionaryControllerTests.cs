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
    public class CustomDictionaryControllerTests
    {
        private CustomDictionaryController customDictionaryController;
        private CustomDictionaryService customDictionaryService;

        [SetUp]
        public void Setup()
        {
            customDictionaryService = new CustomDictionaryService();
            customDictionaryController = new CustomDictionaryController(customDictionaryService);
        }

        [Test]
        public void UploadCustomDictionary_ValidData_ReturnsOk()
        {
            // Arrange
            var additionalVocab = new AdditionalVocab("word");
            var transcriptionConfig = new StartRecognitionMessage_TranscriptionConfig("en", false, new List<AdditionalVocab> { additionalVocab });
            var frontendDictionary = new FrondendDictionary(transcriptionConfig);

            // Act
            var configurationData = new ConfigurationData(frontendDictionary, 2.0f);
            var result = customDictionaryController.UploadCustomDictionary(configurationData);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo("Custom dictionary uploaded successfully."));
        }

        [Test]
        public void UploadCustomDictionary_InvalidData_ReturnsBadRequest()
        {
            // Act with invalid data (for example, passing null)
            var result = customDictionaryController.UploadCustomDictionary(null);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Invalid custom dictionary data."));
        }
    }
}
