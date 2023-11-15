namespace BackendTests;

using Backend.Controllers;
using Backend.Data;
using Backend.Hubs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
    public void ParseFrontendResponseToSpeechBubbleList_InsertValidResponse_AttributesMappedToSpeechBubble()
    {
        // Arrange
        var speechBubbleJson = new SpeechBubbleJson(1, 4, 1, 12, testWordList);
        var speechBubbleChainJson = new SpeechBubbleChainJson(new List<SpeechBubbleJson> { speechBubbleJson });
        var expectedResult = new List<SpeechBubble> { new SpeechBubble(1, 4, 1, 12, testWordList) };

        // Act
        var resultingSpeechBubbleList =
            SpeechBubbleController.ParseFrontendResponseToSpeechBubbleList(speechBubbleChainJson);
        var resultingWordList = resultingSpeechBubbleList.First().SpeechBubbleContent;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(resultingSpeechBubbleList.First().Id, Is.EqualTo(expectedResult.First().Id));
            Assert.That(resultingSpeechBubbleList.First().Speaker, Is.EqualTo(expectedResult.First().Speaker));
            Assert.That(resultingSpeechBubbleList.First().StartTime, Is.EqualTo(expectedResult.First().StartTime));
            Assert.That(resultingSpeechBubbleList.First().EndTime, Is.EqualTo(expectedResult.First().EndTime));

            for (var i = 0; i < resultingWordList.Count; i++)
            {
                Assert.That(resultingWordList[i].Word, Is.EqualTo(testWordList[i].Word));
                Assert.That(resultingWordList[i].StartTime, Is.EqualTo(testWordList[i].StartTime));
                Assert.That(resultingWordList[i].EndTime, Is.EqualTo(testWordList[i].EndTime));
                Assert.That(resultingWordList[i].Speaker, Is.EqualTo(testWordList[i].Speaker));
                Assert.That(resultingWordList[i].Confidence, Is.EqualTo(testWordList[i].Confidence));
            }
        });
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
