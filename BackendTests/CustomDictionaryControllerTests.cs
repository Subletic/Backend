using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Backend.Controllers;

namespace BackendTests
{
    [TestFixture]
    public class CustomDictionaryControllerTests
    {
        private CustomDictionaryController? _customDictionaryController;
        private CustomDictionaryService? _customDictionaryService;

        [SetUp]
        public void Setup()
        {
            _customDictionaryService = new CustomDictionaryService();
            _customDictionaryController = new CustomDictionaryController(_customDictionaryService!);
        }

        [Test]
        public void UploadCustomDictionary_ValidData_ReturnsOk()
        {
            var transcriptionConfig = new TranscriptionConfig("en", new List<AdditionalVocab>());
            var result = _customDictionaryController!.UploadCustomDictionary(transcriptionConfig);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo("Custom dictionary uploaded successfully."));
        }

        [Test]
        public void UploadCustomDictionary_InvalidData_ReturnsBadRequest()
        {
            // Simuliere ungültige Daten
            var result = _customDictionaryController!.UploadCustomDictionary(null!);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Invalid custom dictionary data."));
        }
    }
}
