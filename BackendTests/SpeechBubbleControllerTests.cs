namespace BackendTests;

using Backend.Controllers;
using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Moq;

public class SpeechBubbleControllerTests
{
    private readonly Mock<ISpeechBubbleListService> speechBubbleListService;
    private readonly List<WordToken> testWordList;

    public SpeechBubbleControllerTests()
    {
        speechBubbleListService = new Mock<ISpeechBubbleListService>();
        speechBubbleListService.Setup(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()));
        testWordList = new List<WordToken>
        {
            new("Hello", 1, 1, 10, 4),
            new("World", 1, 11, 12, 4),
            new("!", 1, 12, 12.5, 4),
        };
    }

    [SetUp]
    public void Setup()
    {
        speechBubbleListService.Invocations.Clear();
    }

    [Test]
    public void HandleUpdatedSpeechBubble_ValidData_ReturnsOkResult()
    {
        // Arrange
        var speechBubbleListServiceMock = new Mock<ISpeechBubbleListService>();
        var applicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var controller = new SpeechBubbleController(
            speechBubbleListServiceMock.Object,
            applicationLifetimeMock.Object);

        var speechBubbleChainJson = new SpeechBubbleChainJson
        {
            SpeechbubbleChain = new List<SpeechBubbleJson>()
            {
                new(1, 1, 1, 20, testWordList),
            },
        };

        // Act
        var result = controller.HandleUpdatedSpeechBubble(speechBubbleChainJson);

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public void HandleUpdatedSpeechBubble_NullData_ReturnsBadRequestResult()
    {
        // Arrange
        var speechBubbleListServiceMock = new Mock<ISpeechBubbleListService>();
        var applicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var controller = new SpeechBubbleController(
            speechBubbleListServiceMock.Object,
            applicationLifetimeMock.Object);

        var speechBubbleChainJson = new SpeechBubbleChainJson
        {
            SpeechbubbleChain = null,
        };

        // Act
        var result = controller.HandleUpdatedSpeechBubble(speechBubbleChainJson);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public void HandleRestartRequest_ReturnsOkResult()
    {
        // Arrange
        var speechBubbleListServiceMock = new Mock<ISpeechBubbleListService>();
        var applicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var controller = new SpeechBubbleController(
            speechBubbleListServiceMock.Object,
            applicationLifetimeMock.Object);

        // Act
        var result = controller.HandleRestartRequest();

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
        applicationLifetimeMock.Verify(x => x.StopApplication(), Times.Once);
    }
}
