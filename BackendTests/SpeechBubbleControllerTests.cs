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
        var hubClientsMock = new Mock<IHubClients>();
        
        hubContextMock.Setup(c => c.Clients).Returns(hubClientsMock.Object);

        var controller = new SpeechBubbleController(hubContextMock.Object);
        var testWord = new WordToken(word: "Test", confidence: 0.9f, timeStamp: 1, speaker: 1);
        
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
        var hubClientsMock = new Mock<IHubClients>();
        
        hubContextMock.Setup(c => c.Clients).Returns(hubClientsMock.Object);

        var controller = new SpeechBubbleController(hubContextMock.Object);
        var testWord = new WordToken(word: "Test", confidence: 0.9f, timeStamp: 1, speaker: 1);
        
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
        var hubClientsMock = new Mock<IHubClients>();
        
        hubContextMock.Setup(c => c.Clients).Returns(hubClientsMock.Object);

        var controller = new SpeechBubbleController(hubContextMock.Object);
        var testWord = new WordToken(word: "Test", confidence: 0.9f, timeStamp: 1, speaker: 1);
        
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
        var hubClientsMock = new Mock<IHubClients>();
        
        hubContextMock.Setup(c => c.Clients).Returns(hubClientsMock.Object);

        var controller = new SpeechBubbleController(hubContextMock.Object);
        var testWord = new WordToken(word: "Test", confidence: 0.9f, timeStamp: 1, speaker: 1);
        
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
        var hubClientsMock = new Mock<IHubClients>();
        
        hubContextMock.Setup(c => c.Clients).Returns(hubClientsMock.Object);

        var controller = new SpeechBubbleController(hubContextMock.Object);
        var firstWord = new WordToken(word: "Test", confidence: 0.9f, timeStamp: 1, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, timeStamp: 7, speaker: 1);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, timeStamp: 13, speaker: 1);
        
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
        var hubClientsMock = new Mock<IHubClients>();
        
        hubContextMock.Setup(c => c.Clients).Returns(hubClientsMock.Object);

        var controller = new SpeechBubbleController(hubContextMock.Object);
        var firstWord = new WordToken(word: "Test", confidence: 0.9f, timeStamp: 1, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, timeStamp: 1, speaker: 2);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, timeStamp: 1, speaker: 1);
        
        // Act
        controller.HandleNewWord(firstWord);
        controller.HandleNewWord(secondWord);
        controller.HandleNewWord(thirdWord);
        
        // Assert
        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(2));
    }
    
    [Test]
    public void SpeechBubbleController_Insert4WordsDifferentTimeStampsDifferentSpeakers_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var hubContextMock = new Mock<IHubContext<CommunicationHub>>();
        var hubClientsMock = new Mock<IHubClients>();
        
        hubContextMock.Setup(c => c.Clients).Returns(hubClientsMock.Object);

        var controller = new SpeechBubbleController(hubContextMock.Object);
        var firstWord = new WordToken(word: "Test", confidence: 0.9f, timeStamp: 1, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, timeStamp: 2, speaker: 2);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, timeStamp: 8, speaker: 2);
        var fourthWord = new WordToken(word: "Test4", confidence: 0.7f, timeStamp: 9, speaker: 2);
        
        // Act
        controller.HandleNewWord(firstWord);
        controller.HandleNewWord(secondWord);
        controller.HandleNewWord(thirdWord);
        controller.HandleNewWord(fourthWord);

        // Assert
        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(2));
    }
}