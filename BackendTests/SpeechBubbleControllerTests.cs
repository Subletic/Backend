using Backend.Controllers;
using Backend.Data;
using Backend.Hubs;
using Backend.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace BackendTests;

public class SpeechBubbleControllerTests
{
    private readonly Mock<ISpeechBubbleListService> _speechBubbleListService;
    private readonly List<WordToken> _testWordList;

    public SpeechBubbleControllerTests()
    {
        _speechBubbleListService = new Mock<ISpeechBubbleListService>();
        _speechBubbleListService.Setup(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()));
        _testWordList = new List<WordToken>
        {
            new ("Hello", 1, 1, 10, 4),
            new ("World", 1, 11, 12, 4),
            new ("!", 1, 12, 12.5, 4)
        };
    }

    [SetUp]
    public void Setup()
    {
        _speechBubbleListService.Invocations.Clear();
    }

    [Test]
    public void ParseFrontendResponseToSpeechBubbleList_InsertValidResponse_AttributesMappedToSpeechBubble()
    {
        // Arrange
        var speechBubbleJson = new SpeechBubbleJson(1, 4, 1, 12, _testWordList);
        var speechBubbleChainJson = new SpeechBubbleChainJson(new List<SpeechBubbleJson> { speechBubbleJson });
        var expectedResult = new List<SpeechBubble> { new SpeechBubble(1, 4, 1, 12, _testWordList) };
        
        
        // Act
        var resultingSpeechBubbleList = SpeechBubbleController.ParseFrontendResponseToSpeechBubbleList(speechBubbleChainJson);
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
                Assert.That(resultingWordList[i].Word, Is.EqualTo(_testWordList[i].Word));
                Assert.That(resultingWordList[i].StartTime, Is.EqualTo(_testWordList[i].StartTime));
                Assert.That(resultingWordList[i].EndTime, Is.EqualTo(_testWordList[i].EndTime));
                Assert.That(resultingWordList[i].Speaker, Is.EqualTo(_testWordList[i].Speaker));
                Assert.That(resultingWordList[i].Confidence, Is.EqualTo(_testWordList[i].Confidence));
            }
        });
    }
}