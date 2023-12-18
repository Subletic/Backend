namespace Backend.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Hubs;
using Backend.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;
using Serilog;

[TestFixture]
public class FrontendCommunicationServiceTests
{
    private FrontendCommunicationService service;
    private Mock<ILogger> loggerMock;
    private Mock<IHubContext<FrontendCommunicationHub>> hubContextMock;

    [SetUp]
    public void SetUp()
    {
        loggerMock = new Mock<ILogger>();
        hubContextMock = new Mock<IHubContext<FrontendCommunicationHub>>();
        service = new FrontendCommunicationService(loggerMock.Object, hubContextMock.Object);
    }

    [Test]
    public void Enqueue_ValidItem_ShouldEnqueueItem()
    {
        // Act
        short[] validItem = new short[48000]; // valid length

        // Assert
        Assert.DoesNotThrow(() => service.Enqueue(validItem));
    }

    [Test]
    public void Enqueue_InvalidItem_ShouldThrowArgumentException()
    {
        // Act
        short[] invalidItem = new short[100]; // Invalid length

        // Assert
        Assert.Throws<ArgumentException>(() => service.Enqueue(invalidItem));
    }

    [Test]
    public void TryDequeue_EmptyQueue_ShouldReturnFalse()
    {
        // Act
        bool result = service.TryDequeue(out short[]? dequeuedItem);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(dequeuedItem, Is.Null);
    }

    [Test]
    public void TryDequeue_QueueWithItem_ShouldReturnTrue()
    {
        // Arrange
        short[] expectedItem = new short[48000];
        service.Enqueue(expectedItem);

        // Act
        bool result = service.TryDequeue(out short[]? dequeuedItem);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(dequeuedItem, Is.EqualTo(expectedItem));
    }

    [Test]
    public async Task DeleteSpeechBubble_ValidId_DeletesBubble()
    {
        // mockClients is a mock of IClientProxy that can be used to verify that the hub method is invoked
        var mockClients = new Mock<IClientProxy>();
        hubContextMock.Setup(x => x.Clients.All).Returns(mockClients.Object);

        // service is an instance of FrontendCommunicationService that is being tested
        var service = new FrontendCommunicationService(loggerMock.Object, hubContextMock.Object);

        // id is the ID of the speech bubble that is being deleted
        var id = 123;

        // When the DeleteSpeechBubble method is called with a valid ID, the hub method is invoked with the ID
        await service.DeleteSpeechBubble(id);

        // Verify that the hub method was invoked with the correct parameters
        mockClients.Verify(x => x.SendAsync("deleteBubble", id, default(CancellationToken)), Times.Once);
    }

    [Test]
    public async Task PublishSpeechBubble_WhenCalled_ShouldInvokeHubMethod()
    {
        // wordTokens is a list of WordToken objects that represent the words in the speech bubble
        var wordTokens = new List<WordToken>
        {
            new WordToken("example", 0.1f, 0.0, 1.0, 1),
        };

        // speechBubble is an object that represents the speech bubble to be sent to the frontend
        var speechBubble = new SpeechBubble(1, 1, 0.0, 1.0, wordTokens);

        // mockClients is a mock of IClientProxy that can be used to verify that the hub method is invoked
        var mockClients = new Mock<IClientProxy>();
        hubContextMock.Setup(x => x.Clients.All).Returns(mockClients.Object);

        // When the PublishSpeechBubble method is called, it should invoke the hub method with the speechBubble object
        await service.PublishSpeechBubble(speechBubble);

        // Verify that the hub method was invoked with the correct parameters
        mockClients.Verify(
            x => x.SendAsync(
                "newBubble",
                It.Is<object[]>(args => args.Length == 1 && args[0] as List<SpeechBubble> != null && ((List<SpeechBubble>)args[0]).Contains(speechBubble)),
                default(CancellationToken)),
            Times.Once);
    }
}
