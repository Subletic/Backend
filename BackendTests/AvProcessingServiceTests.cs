using Backend.Services;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BackendTests
{
    public class AvProcessingServiceTests
    {
        private readonly Mock<IWordProcessingService> wordProcessingServiceMock;
        private readonly Mock<FrontendAudioQueueService> frontendAudioQueueServiceMock;

        public AvProcessingServiceTests()
        {
            wordProcessingServiceMock = new Mock<IWordProcessingService>();
            frontendAudioQueueServiceMock = new Mock<FrontendAudioQueueService>();
        }

        [Test]
        public void Init_WithValidApiKey_ReturnsTrue()
        {
            // Arrange
            var avProcessingService = new AvProcessingService(wordProcessingServiceMock.Object, frontendAudioQueueServiceMock.Object);
            const string apiKeyVar = "API_KEY";
            Environment.SetEnvironmentVariable(apiKeyVar, "eHbFYSRbfbTyORS3cs3HmguSCL9XMbbv");

            // Act
            var result = avProcessingService.Init(apiKeyVar);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Init_WithNoApiKey_ReturnsFalse()
        {
            // Arrange
            var avProcessingService = new AvProcessingService(wordProcessingServiceMock.Object, frontendAudioQueueServiceMock.Object);
            const string apiKeyVar = "API_KEY";
            Environment.SetEnvironmentVariable(apiKeyVar, null);

            // Act
            var result = avProcessingService.Init(apiKeyVar);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TranscribeAudio_WithInvalidApiKey_ReturnsFalse()
        {
            // Arrange
            var avProcessingService = new AvProcessingService(wordProcessingServiceMock.Object, frontendAudioQueueServiceMock.Object);
            Stream audioStream = new MemoryStream(); // Erstellen Sie hier einen geeigneten Stream

            // Act
            var result = await avProcessingService.TranscribeAudio(audioStream);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
