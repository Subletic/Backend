namespace BackendTests;

using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.ClientCommunication;
using Backend.Data;
using Backend.FrontendCommunication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

public class SubtitleExporterServiceTests
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

    [TestCase("vtt", 2)]
    [TestCase("srt", 1)]
    public async Task Start_SendsSubtitlesOverWebSocket(string format, int amountOfWrites)
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Debug)
            .WriteTo.Console()
            .CreateLogger();

        Mock<IFrontendCommunicationService> mockFrontendCommunicationService = new Mock<IFrontendCommunicationService>();

        var service = new SubtitleExporterService(mockFrontendCommunicationService.Object, configuration, logger);

        // Select the format to initialize the subtitleConverter
        service.SelectFormat(format);

        var mockWebSocket = new Mock<WebSocket>();
        var cancellationTokenSource = new CancellationTokenSource();

        // Generate some sample subtitle data
        var wordTokens = new List<WordToken>
        {
            new WordToken("Hello", 0.95f, 0.0, 1.0, 1),
            new WordToken("world", 0.92f, 1.0, 2.0, 1),
            new WordToken("of", 0.91f, 2.0, 3.0, 1),
            new WordToken("unit", 0.93f, 3.0, 4.0, 1),
            new WordToken("testing", 0.94f, 4.0, 5.0, 1),
        };

        var speechBubble = new SpeechBubble(1, 1, 0.0, 5.0, wordTokens);

        // Act: Start sending task
        var sendingTask = service.Start(mockWebSocket.Object, cancellationTokenSource);

        // Allow some time for reading loop to start
        await Task.Delay(100);

        // Act: Inform service that there will be no more subtitles beyond this one, and that it should finish
        service.SetQueueContainsItems(false);
        service.RequestShutdown();

        // Act: Export the subtitle
        await service.ExportSubtitle(speechBubble);

        // Act: Await service to finish
        sendingTask.Wait(2000); // Wait for the sending task to complete

        // Assert
        Assert.That(
            mockWebSocket.Invocations.Count(
                x => x.Method.Name.Equals(nameof(WebSocket.SendAsync))),
            Is.EqualTo(amountOfWrites));
        Assert.That(
            mockFrontendCommunicationService.Invocations.Count(
                x => x.Method.Name.Equals(nameof(IFrontendCommunicationService.AbortCorrection))),
            Is.EqualTo(0));
    }
}
