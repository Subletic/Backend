using Backend.Services;
using Moq;

namespace BackendTests
{
    public class AvProcessingServiceTests
    {
        private readonly Mock<IWordProcessingService> _wordProcessingServiceMock;
        private readonly Mock<FrontendAudioQueueService> _frontendAudioQueueServiceMock;

        public AvProcessingServiceTests()
        {
            _wordProcessingServiceMock = new Mock<IWordProcessingService>();
            _frontendAudioQueueServiceMock = new Mock<FrontendAudioQueueService>();
        }

        [Test]
        public async Task Init_WithValidApiKey_ReturnsTrue()
        {
            // Arrange
            var avProcessingService = new AvProcessingService(_wordProcessingServiceMock.Object,
                _frontendAudioQueueServiceMock.Object, new WebVttExporter(new MemoryStream()));
            const string apiKeyVar = "API_KEY";
            Environment.SetEnvironmentVariable(apiKeyVar, "eHbFYSRbfbTyORS3cs3HmguSCL9XMbbv");

            // Act
            var result = await avProcessingService.Init(apiKeyVar);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Init_WithInvalidApiKey_ThrowsException()
        {
            // Arrange
            var avProcessingService = new AvProcessingService(_wordProcessingServiceMock.Object,
                _frontendAudioQueueServiceMock.Object, new WebVttExporter(new MemoryStream()));
            const string apiKeyVar = "API_KEY";
            Environment.SetEnvironmentVariable(apiKeyVar, "invalidKey");

            // Assert
            Assert.That(() => avProcessingService.Init(apiKeyVar), Throws.InvalidOperationException);
        }

        [Test]
        public async Task Init_WithNoApiKey_ReturnsFalse()
        {
            // Arrange
            var avProcessingService = new AvProcessingService(_wordProcessingServiceMock.Object,
                _frontendAudioQueueServiceMock.Object, new WebVttExporter(new MemoryStream()));
            const string apiKeyVar = "API_KEY";
            Environment.SetEnvironmentVariable(apiKeyVar, null);

            // Act
            var result = await avProcessingService.Init(apiKeyVar);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TranscribeAudio_WithInvalidApiKey_ReturnsFalse()
        {
            // Arrange
            var avProcessingService = new AvProcessingService(_wordProcessingServiceMock.Object,
                _frontendAudioQueueServiceMock.Object, new WebVttExporter(new MemoryStream()));
            Uri filePath = new Uri ("file://unnecessaryPath");

            // Act
            var result = await avProcessingService.TranscribeAudio(filePath);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
