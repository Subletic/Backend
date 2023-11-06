using Backend.Data;
using Backend.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BackendTests
{
    public class SubtitleExporterServiceTests
    {
        [Test]
        public async Task Start_SendsSubtitlesOverWebSocket()
        {
            // Arrange
            var service = new SubtitleExporterService();

            var mockWebSocket = new Mock<WebSocket>();
            var cancellationTokenSource = new CancellationTokenSource();

            // Generate some sample subtitle data
            var wordTokens = new List<WordToken>
            {
                new WordToken("Hello", 0.95f, 0.0, 1.0, 1),
                new WordToken("world", 0.92f, 1.0, 2.0, 1),
                new WordToken("of", 0.91f, 2.0, 3.0, 1),
                new WordToken("unit", 0.93f, 3.0, 4.0, 1),
                new WordToken("testing", 0.94f, 4.0, 5.0, 1)
            };

            var speechBubble = new SpeechBubble(1, 1, 0.0, 5.0, wordTokens);

            // Act: Start sending task
            var sendingTask = service.Start(mockWebSocket.Object, cancellationTokenSource);

            // Act: Export the subtitle
            await service.ExportSubtitle(speechBubble);

            // Allow time for data to be sent
            await Task.Delay(100);

            // Clean up
            cancellationTokenSource.Cancel();

            // Assert
            sendingTask.Wait(2000); // Wait for the sending task to complete
            mockWebSocket.Verify(webSocket =>
                webSocket.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), WebSocketMessageType.Text, false, cancellationTokenSource.Token),
                Times.AtLeastOnce());
        }
    }
}
