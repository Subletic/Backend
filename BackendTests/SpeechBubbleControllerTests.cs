using Backend.Controllers;
using Backend.Data;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace BackendTests;

public class SpeechBubbleControllerTests
{
    [Test]
    public void SpeechBubbleController_Insert19NewWords_SpeechBubbleListEmpty()
    {
        // Arrange
        var hubContextMock = new Mock<IHubContext<CommunicationHub>>();
        var controller = new SpeechBubbleController(hubContextMock.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 19; i++)
        {
            controller.HandleNewWord(testWord);
        }

        // Assert
        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(0));
    }

    [Test]
    public void SpeechBubbleController_Insert20NewWords_FirstSpeechBubbleAvailable()
    {
        // Arrange
        var hubContextMock = new Mock<IHubContext<CommunicationHub>>();
        var controller = new SpeechBubbleController(hubContextMock.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 20; i++)
        {
            controller.HandleNewWord(testWord);
        }

        // Assert
        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(1));
    }


    [Test]
    public void SpeechBubbleController_Insert40NewWords_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var hubContextMock = new Mock<IHubContext<CommunicationHub>>();
        var controller = new SpeechBubbleController(hubContextMock.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 40; i++)
        {
            controller.HandleNewWord(testWord);
        }

        // Assert
        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(2));
    }

    [Test]
    public void SpeechBubbleController_Insert120NewWords_SpeechBubbleListContains6Bubbles()
    {
        // Arrange
        var hubContextMock = new Mock<IHubContext<CommunicationHub>>();
        var controller = new SpeechBubbleController(hubContextMock.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 120; i++)
        {
            controller.HandleNewWord(testWord);
        }

        // Assert
        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(6));
    }

    [Test]
    public void SpeechBubbleController_Insert3WordsSeperated6Seconds_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var hubContextMock = new Mock<IHubContext<CommunicationHub>>();
        var controller = new SpeechBubbleController(hubContextMock.Object);

        var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 0, endTime: 1, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 7, endTime: 8, speaker: 1);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 14, endTime: 19, speaker: 1);

        // Act
        controller.HandleNewWord(firstWord);
        controller.HandleNewWord(secondWord);
        controller.HandleNewWord(thirdWord);

        // Assert
        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(2));
    }

    [Test]
    public void SpeechBubbleController_Insert3WordsDifferentSpeakers_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var hubContextMock = new Mock<IHubContext<CommunicationHub>>();
        var controller = new SpeechBubbleController(hubContextMock.Object);

        var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1, endTime: 3, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 4, endTime: 5,speaker: 2);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 6, endTime: 7,speaker: 1);

        // Act
        controller.HandleNewWord(firstWord);
        controller.HandleNewWord(secondWord);
        controller.HandleNewWord(thirdWord);

        // Assert
        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(2));
    }

    [Test]
    public void
        SpeechBubbleController_Insert4WordsDifferentTimeStampsDifferentSpeakers_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var hubContextMock = new Mock<IHubContext<CommunicationHub>>();
        var controller = new SpeechBubbleController(hubContextMock.Object);

        var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1, endTime: 2, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 3, endTime: 4, speaker: 2);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 10, endTime: 11, speaker: 2);
        var fourthWord = new WordToken(word: "Test4", confidence: 0.7f, startTime: 12, endTime: 14, speaker: 2);

        // Act
        controller.HandleNewWord(firstWord);
        controller.HandleNewWord(secondWord);
        controller.HandleNewWord(thirdWord);
        controller.HandleNewWord(fourthWord);

        // Assert
        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(2));
    }
}
