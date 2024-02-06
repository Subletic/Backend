namespace BackendTests;

using Backend.ClientCommunication;
using Backend.Data;
using Backend.FrontendCommunication;
using Backend.SpeechBubble;
using Microsoft.AspNetCore.SignalR;
using Moq;

public class WordProcessingServiceTests
{
    private readonly Mock<IFrontendCommunicationService> frontendCommunicationServiceMock;
    private readonly Mock<IHubContext<FrontendCommunicationHub>> hubContextMock;
    private readonly Mock<ISpeechBubbleListService> speechBubbleListService;

    public WordProcessingServiceTests()
    {
        frontendCommunicationServiceMock = new Mock<IFrontendCommunicationService>();
        hubContextMock = new Mock<IHubContext<FrontendCommunicationHub>>();
        speechBubbleListService = new Mock<ISpeechBubbleListService>();
        speechBubbleListService.Setup(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()));
    }

    [SetUp]
    public void Setup()
    {
        speechBubbleListService.Invocations.Clear();
    }

    [Test]
    public async Task Insert19NewWords_SpeechBubbleListEmpty()
    {
        // Arrange
        var controller = new WordProcessingService(frontendCommunicationServiceMock.Object, hubContextMock.Object, speechBubbleListService.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 19; i++)
        {
            await controller.HandleNewWord(testWord);
        }

        // Assert
        speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(0));
    }

    [Test]
    public async Task Insert20NewWords_FirstSpeechBubbleAvailable()
    {
        // Arrange
        var controller = new WordProcessingService(frontendCommunicationServiceMock.Object, hubContextMock.Object, speechBubbleListService.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 20; i++)
        {
            await controller.HandleNewWord(testWord);
        }

        // Assert
        speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(1));
    }

    [Test]
    public async Task Insert40NewWords_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var controller = new WordProcessingService(frontendCommunicationServiceMock.Object, hubContextMock.Object, speechBubbleListService.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 40; i++)
        {
            await controller.HandleNewWord(testWord);
        }

        // Assert
        speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(2));
    }

    [Test]
    public async Task Insert120NewWords_SpeechBubbleListContains6Bubbles()
    {
        // Arrange
        var controller = new WordProcessingService(frontendCommunicationServiceMock.Object, hubContextMock.Object, speechBubbleListService.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 120; i++)
        {
            await controller.HandleNewWord(testWord);
        }

        // Assert
        speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(6));
    }

    [Test]
    public async Task Insert3WordsSeperated6Seconds_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var controller = new WordProcessingService(frontendCommunicationServiceMock.Object, hubContextMock.Object, speechBubbleListService.Object);

        var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 0, endTime: 1, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 7, endTime: 8, speaker: 1);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 14, endTime: 19, speaker: 1);

        // Act
        await controller.HandleNewWord(firstWord);
        await controller.HandleNewWord(secondWord);
        await controller.HandleNewWord(thirdWord);

        // Assert
        speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(2));
    }

    [Test]
    public async Task Insert3WordsDifferentSpeakers_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var controller = new WordProcessingService(frontendCommunicationServiceMock.Object, hubContextMock.Object, speechBubbleListService.Object);

        var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1, endTime: 3, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 4, endTime: 5, speaker: 2);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 6, endTime: 7, speaker: 1);

        // Act
        await controller.HandleNewWord(firstWord);
        await controller.HandleNewWord(secondWord);
        await controller.HandleNewWord(thirdWord);

        // Assert
        speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(2));
    }

    [Test]
    public async Task Insert4WordsDifferentTimeStampsDifferentSpeakers_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var controller = new WordProcessingService(frontendCommunicationServiceMock.Object, hubContextMock.Object, speechBubbleListService.Object);

        var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1, endTime: 2, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 3, endTime: 4, speaker: 2);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 10, endTime: 11, speaker: 2);
        var fourthWord = new WordToken(word: "Test4", confidence: 0.7f, startTime: 12, endTime: 14, speaker: 2);

        // Act
        await controller.HandleNewWord(firstWord);
        await controller.HandleNewWord(secondWord);
        await controller.HandleNewWord(thirdWord);
        await controller.HandleNewWord(fourthWord);

        // Assert
        speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(2));
    }

    [Test]
    public async Task InsertCommaAfterWord_OneSpeechBubbleGenerated()
    {
        // Arrange
        var controller = new WordProcessingService(frontendCommunicationServiceMock.Object, hubContextMock.Object, speechBubbleListService.Object);

        var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1, endTime: 2, speaker: 1);
        var secondWord = new WordToken(word: ",", confidence: 0.7f, startTime: 7, endTime: 9, speaker: 1);
        var thirdWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 15, endTime: 17, speaker: 1);

        // Act
        await controller.HandleNewWord(firstWord);
        await controller.HandleNewWord(secondWord);
        await controller.HandleNewWord(thirdWord);

        // Assert
        speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(1));
    }
}
