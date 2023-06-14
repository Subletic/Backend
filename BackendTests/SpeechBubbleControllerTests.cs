﻿using Backend.Controllers;
using Backend.Data;
using Backend.Hubs;
using Backend.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace BackendTests;

public class SpeechBubbleControllerTests
{
    private readonly Mock<IHubContext<CommunicationHub>> _hubContextMock;
    private readonly Mock<ISpeechBubbleListService> _speechBubbleListService;

    public SpeechBubbleControllerTests()
    {
        _hubContextMock = new Mock<IHubContext<CommunicationHub>>();
        _speechBubbleListService = new Mock<ISpeechBubbleListService>();
        _speechBubbleListService.Setup(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()));
    }

    [SetUp]
    public void Setup()
    {
        _speechBubbleListService.Invocations.Clear();
    }


    [Test]
    public void Insert19NewWords_SpeechBubbleListEmpty()
    {
        // Arrange
        var controller = new SpeechBubbleController(_hubContextMock.Object, _speechBubbleListService.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 19; i++)
        {
            controller.HandleNewWord(testWord);
        }

        // Assert
        _speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(0));
    }

    [Test]
    public void Insert20NewWords_FirstSpeechBubbleAvailable()
    {
        // Arrange
        var controller = new SpeechBubbleController(_hubContextMock.Object, _speechBubbleListService.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 20; i++)
        {
            controller.HandleNewWord(testWord);
        }

        // Assert
        _speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(1));
    }


    [Test]
    public void Insert40NewWords_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var controller = new SpeechBubbleController(_hubContextMock.Object, _speechBubbleListService.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 40; i++)
        {
            controller.HandleNewWord(testWord);
        }

        // Assert
        _speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(2));
    }

    [Test]
    public void Insert120NewWords_SpeechBubbleListContains6Bubbles()
    {
        // Arrange
        var controller = new SpeechBubbleController(_hubContextMock.Object, _speechBubbleListService.Object);

        var testWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1.0, endTime: 2.0, speaker: 1);

        // Act
        for (var i = 0; i < 120; i++)
        {
            controller.HandleNewWord(testWord);
        }

        // Assert
        _speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(6));
    }

    [Test]
    public void Insert3WordsSeperated6Seconds_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var controller = new SpeechBubbleController(_hubContextMock.Object, _speechBubbleListService.Object);

        var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 0, endTime: 1, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 7, endTime: 8, speaker: 1);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 14, endTime: 19, speaker: 1);

        // Act
        controller.HandleNewWord(firstWord);
        controller.HandleNewWord(secondWord);
        controller.HandleNewWord(thirdWord);

        // Assert
        _speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(2));
    }

    [Test]
    public void Insert3WordsDifferentSpeakers_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var controller = new SpeechBubbleController(_hubContextMock.Object, _speechBubbleListService.Object);

        var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1, endTime: 3, speaker: 1);
        var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 4, endTime: 5, speaker: 2);
        var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 6, endTime: 7, speaker: 1);

        // Act
        controller.HandleNewWord(firstWord);
        controller.HandleNewWord(secondWord);
        controller.HandleNewWord(thirdWord);

        // Assert
        _speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(2));
    }

    [Test]
    public void Insert4WordsDifferentTimeStampsDifferentSpeakers_SpeechBubbleListContains2Bubbles()
    {
        // Arrange
        var controller = new SpeechBubbleController(_hubContextMock.Object, _speechBubbleListService.Object);

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
        _speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(2));
    }

    [Test]
    public void InsertCommaAfterWord_OneSpeechBubbleGenerated()
    {
        // Arrange
        var controller = new SpeechBubbleController(_hubContextMock.Object, _speechBubbleListService.Object);
        
        var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1, endTime: 2, speaker: 1);
        var secondWord = new WordToken(word: ",", confidence: 0.7f, startTime: 7, endTime: 9, speaker: 1);
        var thirdWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 15, endTime: 17, speaker: 1);
        
        // Act
        controller.HandleNewWord(firstWord);
        controller.HandleNewWord(secondWord);
        controller.HandleNewWord(thirdWord);
        
        // Assert
        _speechBubbleListService.Verify(sl => sl.AddNewSpeechBubble(It.IsAny<SpeechBubble>()), Times.Exactly(1));
    }
    
}