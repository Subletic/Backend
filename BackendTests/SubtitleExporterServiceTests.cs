using Backend.Services;
using Moq;
using NUnit.Framework;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
namespace BackendTests
{
    public class SubtitleExporterServiceTests
    {
        [Test]
        public async Task Start_SendsSubtitlesOverWebSocket()
        {
            // Arrange
            var service = new SubtitleExporterService();

            // Mock für WebSocket erstellen
            var mockWebSocket = new Mock<WebSocket>();

            // Mock für CancellationTokenSource erstellen
            var mockCancellationTokenSource = new Mock<CancellationTokenSource>();

            // Setzen der Verhalten für CancellationTokenSource
            mockCancellationTokenSource.Setup(m => m.Token).Returns(CancellationToken.None);

            // Simulieren des ExportSubtitle-Aufrufs
            await service.ExportSubtitle(new SpeechBubble(1, 1, 0.0, 1.0, new List<WordToken>()));

            // Act
            var sendingTask = service.Start(mockWebSocket.Object, mockCancellationTokenSource.Object);

            // Assert - Überprüfen Sie, ob die WebSocket-Methode aufgerufen wurde
            mockWebSocket.Verify(webSocket =>
                webSocket.SendAsync(It.IsAny<Memory<byte>>(), WebSocketMessageType.Text, true, CancellationToken.None),
                Times.AtLeastOnce());

            // Überprüfen Sie, ob die Methode ohne Ausnahmen abgeschlossen wurde
            sendingTask.Wait();
        }
    }
}
