using System;
using System.IO;
using System.Threading.Tasks;
using Backend.Services;
using Moq;
using NUnit.Framework;

public class AvProcessingServiceTests
{
    private readonly Mock<IWordProcessingService> wordProcessingServiceMock;
    private readonly Mock<IFrontendCommunicationService> frontendCommunicationServiceMock;

    public AvProcessingServiceTests()
    {
        wordProcessingServiceMock = new Mock<IWordProcessingService>();
        frontendCommunicationServiceMock = new Mock<IFrontendCommunicationService>();
    }

    [Test]
    public void Init_WithValidApiKey_ReturnsTrue()
    {
        // Arrange
        var avProcessingService = new AvProcessingService(wordProcessingServiceMock.Object, frontendCommunicationServiceMock.Object);
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
        var avProcessingService = new AvProcessingService(wordProcessingServiceMock.Object, frontendCommunicationServiceMock.Object);
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
        var avProcessingService = new AvProcessingService(wordProcessingServiceMock.Object, frontendCommunicationServiceMock.Object);
        Stream audioStream = new MemoryStream(); // Create a suitable stream

        // Act
        var result = await avProcessingService.TranscribeAudio(audioStream);

        // Assert
        Assert.That(result, Is.False);
    }
}
