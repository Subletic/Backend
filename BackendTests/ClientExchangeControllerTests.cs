namespace BackendTests;

using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;
using Serilog;

public class ClientExchangeControllerTests
{
    private readonly IConfiguration configuration = new ConfigurationBuilder()
        .Add(new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("ClientCommunicationSettings:TIMEOUT_IN_SECONDS", "1"),
            },
        })
        .Build();

    private readonly Mock<ILogger> mockLogger = new Mock<ILogger>();
    private readonly Mock<ISubtitleExporterService> mockSubtitleExporterService = new Mock<ISubtitleExporterService>();
    private readonly Mock<IAvReceiverService> mockAvReceiverService = new Mock<IAvReceiverService>();
    private readonly Mock<ISpeechmaticsConnectionService> mockSpeechmaticsConnectionService = new Mock<ISpeechmaticsConnectionService>();
    private readonly Mock<ISpeechmaticsReceiveService> mockSpeechmaticsReceiveService = new Mock<ISpeechmaticsReceiveService>();
    private readonly Mock<ISpeechmaticsSendService> mockSpeechmaticsSendService = new Mock<ISpeechmaticsSendService>();
    private readonly Mock<ISpeechBubbleListService> mockSpeechBubbleListService = new Mock<ISpeechBubbleListService>();
    private readonly Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
    private readonly Mock<WebSocketManager> mockWebSocketManager = new Mock<WebSocketManager>();
    private readonly Mock<WebSocket> mockWebSocket = new Mock<WebSocket>();

    private readonly Mock<IConfigurationService> configurationServiceMock = new Mock<IConfigurationService>();

    public ClientExchangeControllerTests()
    {
        configurationServiceMock.Setup(m => m.GetCustomDictionary()).Returns(new StartRecognitionMessage_TranscriptionConfig());
        mockHttpContext.Setup(c => c.WebSockets).Returns(mockWebSocketManager.Object);
    }

    [Test]
    public async Task Get_ShouldHandleWebSocketRequest()
    {
        // Arrange
        mockWebSocketManager.Setup(m => m.IsWebSocketRequest).Returns(true);
        mockWebSocketManager.Setup(m => m.AcceptWebSocketAsync()).ReturnsAsync(mockWebSocket.Object);
        var controller = new ClientExchangeController(
        mockAvReceiverService.Object,
        mockSpeechBubbleListService.Object,
        mockSpeechmaticsConnectionService.Object,
        mockSpeechmaticsReceiveService.Object,
        mockSpeechmaticsSendService.Object,
        mockSubtitleExporterService.Object,
        configuration,
        configurationServiceMock.Object,
        mockLogger.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object },
        };

        // Act
        await controller.Get();

        // Assert
        mockWebSocketManager.Verify(m => m.AcceptWebSocketAsync(), Times.Once);
    }
}
