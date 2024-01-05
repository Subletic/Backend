namespace BackendTests;

using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Serilog;
using Xunit;
using NUnitTheory = NUnit.Framework.TheoryAttribute;
using XunitTheory = Xunit.TheoryAttribute;

public class ClientExchangeControllerTests
{
    private readonly Mock<ILogger> mockLogger = new Mock<ILogger>();
    private readonly Mock<ISubtitleExporterService> mockSubtitleExporterService = new Mock<ISubtitleExporterService>();
    private readonly Mock<IAvReceiverService> mockAvReceiverService = new Mock<IAvReceiverService>();
    private readonly Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
    private readonly Mock<WebSocketManager> mockWebSocketManager = new Mock<WebSocketManager>();
    private readonly Mock<WebSocket> mockWebSocket = new Mock<WebSocket>();

    public ClientExchangeControllerTests()
    {
        mockHttpContext.Setup(c => c.WebSockets).Returns(mockWebSocketManager.Object);
    }

    [Fact]
    public async Task Get_ShouldHandleWebSocketRequest()
    {
        // Arrange
        mockWebSocketManager.Setup(m => m.IsWebSocketRequest).Returns(true);
        mockWebSocketManager.Setup(m => m.AcceptWebSocketAsync()).ReturnsAsync(mockWebSocket.Object);
        var controller = new ClientExchangeController(mockLogger.Object, mockSubtitleExporterService.Object, mockAvReceiverService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object },
        };

        // Act
        await controller.Get();

        // Assert
        mockWebSocketManager.Verify(m => m.AcceptWebSocketAsync(), Times.Once);
        mockSubtitleExporterService.Verify(s => s.Start(It.IsAny<WebSocket>(), It.IsAny<CancellationTokenSource>()), Times.Once);
        mockAvReceiverService.Verify(a => a.Start(It.IsAny<WebSocket>(), It.IsAny<CancellationTokenSource>()), Times.Once);
    }

    [Fact]
    public async Task Get_ShouldCorrectlyReceiveAndProcessFormatSpecification()
    {
        // Arrange
        var mockWebSocket = new Mock<WebSocket>();
        var mockWebSocketManager = new Mock<WebSocketManager>();
        var mockHttpContext = new Mock<HttpContext>();

        mockWebSocketManager.Setup(m => m.IsWebSocketRequest).Returns(true);
        mockWebSocketManager.Setup(m => m.AcceptWebSocketAsync()).ReturnsAsync(mockWebSocket.Object);
        mockHttpContext.Setup(c => c.WebSockets).Returns(mockWebSocketManager.Object);

        var controller = new ClientExchangeController(mockLogger.Object, mockSubtitleExporterService.Object, mockAvReceiverService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object },
        };

        // Simuliere das Senden einer korrekten Format-Nachricht
        var buffer = Encoding.UTF8.GetBytes("{\"format\":\"webvtt\"}");
        var segment = new ArraySegment<byte>(buffer);
        mockWebSocket.Setup(ws => ws.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, true));

        // Act
        await controller.Get();

        // Assert
        mockSubtitleExporterService.Verify(s => s.SelectFormat(It.Is<string>(format => format == "webvtt")), Times.Once, "Die Methode SelectFormat wurde nicht mit dem erwarteten Format aufgerufen.");
        mockSubtitleExporterService.Verify(s => s.Start(It.IsAny<WebSocket>(), It.IsAny<CancellationTokenSource>()), Times.Once, "Die Methode Start wurde nicht auf dem subtitleExporterService aufgerufen.");
        mockAvReceiverService.Verify(a => a.Start(It.IsAny<WebSocket>(), It.IsAny<CancellationTokenSource>()), Times.Once, "Die Methode Start wurde nicht auf dem avReceiverService aufgerufen.");

        // Zusätzlich kannst du überprüfen, ob keine unerwarteten Fehler aufgetreten sind
        mockLogger.Verify(l => l.Error(It.IsAny<string>()), Times.Never, "Es sollte kein Fehler geloggt werden.");
    }

    [XunitTheory]
    [InlineData("webvtt", true)]
    [InlineData("srt", true)]
    [InlineData("invalidFormat", false)]
    public void IsValidFormat_ShouldCheckSubtitleFormatValidity(string format, bool expected)
    {
        // Arrange
        var method = typeof(ClientExchangeController).GetMethod("isValidFormat", BindingFlags.NonPublic | BindingFlags.Static);

        // Stelle sicher, dass die Methode nicht null ist, bevor du fortfährst.
        if (method == null)
        {
            throw new InvalidOperationException("Die Methode 'isValidFormat' konnte nicht gefunden werden.");
        }

        // Act
        var result = method.Invoke(null, new object[] { format });

        // Assert
        Assert.Equal(expected, result);
    }
}
