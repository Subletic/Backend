using Backend.Controllers;
using Backend.Data;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
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
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 4, endTime: 5, speaker: 2);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 6, endTime: 7, speaker: 1);

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


   
    [Test]
    public void HandleUpdatedSpeechBubble_ExistingSpeechBubble_UpdatesAndReturnsUpdatedList()
    {
        // Arrange
        var hubContextMock = new Mock<IHubContext<CommunicationHub>>();
        var controller = new SpeechBubbleController(hubContextMock.Object);

        // Erstellen der ursprünglichen Speech Bubble
        var existingSpeechBubble = new SpeechBubble(1, 1, 10.0, 15.0, new List<WordToken>());
        var firstWord = new WordToken(word: "Test1", confidence: 0.9f, startTime: 1, endTime: 2, speaker: 1);
        controller.HandleNewWord(firstWord);
        controller.AddSpeechBubble(existingSpeechBubble);

        // Erstellen der aktualisierten Speech Bubble
        var updatedSpeechBubble = new SpeechBubble(1, 2, 5.0, 20.0, new List<WordToken>());
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 3, endTime: 4, speaker: 2);
        controller.HandleNewWord(secondWord);

        // Act
        var result = controller.HandleUpdatedSpeechBubble(updatedSpeechBubble);

        // Assert
        // Überprüfen, ob die Rückgabe ein OkObjectResult ist
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;

        // Überprüfen, ob die zurückgegebene Liste der Speech Bubbles korrekt ist
        Assert.That(okResult.Value, Is.EqualTo(controller.GetSpeechBubbles()));

        // Überprüfen, ob die ursprüngliche Speech Bubble korrekt aktualisiert wurde
        SpeechBubble updatedBubble = controller.GetSpeechBubbles().First(b => b.Id == existingSpeechBubble.Id);
        Assert.That(updatedBubble.StartTime, Is.EqualTo(updatedSpeechBubble.StartTime));
        Assert.That(updatedBubble.EndTime, Is.EqualTo(updatedSpeechBubble.EndTime));
        Assert.That(updatedBubble.Speaker, Is.EqualTo(updatedSpeechBubble.Speaker));
        Assert.That(updatedBubble.SpeechBubbleContent, Is.EqualTo(updatedSpeechBubble.SpeechBubbleContent));

        // Überprüfen, ob die nicht aktualisierten Speech Bubbles unverändert bleiben
        var nonUpdatedBubbles = controller.GetSpeechBubbles().Where(b => b.Id != existingSpeechBubble.Id);
        foreach (var bubble in nonUpdatedBubbles)
        {
            Assert.That(bubble.StartTime, Is.Not.EqualTo(updatedSpeechBubble.StartTime));
            Assert.That(bubble.EndTime, Is.Not.EqualTo(updatedSpeechBubble.EndTime));
            Assert.That(bubble.Speaker, Is.Not.EqualTo(updatedSpeechBubble.Speaker));
            Assert.That(bubble.SpeechBubbleContent, Is.Not.EqualTo(updatedSpeechBubble.SpeechBubbleContent));
        }
    }

}
