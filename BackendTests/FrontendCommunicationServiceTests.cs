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
    private IFrontendCommunicationService service = null!;
    private Mock<ILogger> loggerMock;
    private Mock<IHubContext<FrontendCommunicationHub>> hubContextMock;

    [SetUp]
    public void SetUp()
    {
        loggerMock = new Mock<ILogger>();
        hubContextMock = new Mock<IHubContext<FrontendCommunicationHub>>();
        service = new FrontendCommunicationService(loggerMock.Object, hubContextMock.Object);
        service.ResetAbortedTracker();
    }

    [Test]
    public void Enqueue_ValidItem_ShouldEnqueueItem()
    {
        // Act
        short[] validItem = new short[48000]; // valid length

        // Assert
        Assert.IsNotNull(service, "Service ist null.");
        Assert.DoesNotThrow(() => service.Enqueue(validItem));
    }

    [Test]
    public void Enqueue_InvalidItem_ShouldThrowArgumentException()
    {
        // Act
        short[] invalidItem = new short[100]; // Invalid length

        // Assert
        Assert.IsNotNull(service, "Service ist null.");
        Assert.Throws<ArgumentException>(() => service.Enqueue(invalidItem));
    }

    [Test]
    public void TryDequeue_EmptyQueue_ShouldReturnFalse()
    {
        // Act
        Assert.IsNotNull(service, "Service ist null.");
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
        Assert.IsNotNull(service, "Service ist null.");
        bool result = service.TryDequeue(out short[]? dequeuedItem);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(dequeuedItem, Is.EqualTo(expectedItem));
    }
}
