using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Services;
using Moq;
using Xunit;

/// <summary>
/// Dies ist die Testklasse für die SubtitleExporter-Klasse.
/// </summary>

public class SubtitleExporterTests
{
    [Fact]
    public async Task ExportSubtitle_SendsSubtitleToWebSocket()
    {
        // Arrange
        var mockWebSocket = new Mock<ClientWebSocket>();
        var mockSubtitleConverter = new Mock<ISubtitleConverter>();

        // Erstellen Sie eine Instanz des SubtitleExporter und übergeben Sie die Mock-Abhängigkeiten
        var subtitleExporter = new SubtitleExporter(mockWebSocket.Object, mockSubtitleConverter.Object);

        // Initialisieren Sie eine SpeechBubble mit den erforderlichen Parametern
        var speechBubble = new SpeechBubble(
            id: 1,
            speaker: 2,
            startTime: 3.0,
            endTime: 4.0,
            wordTokens: new List<WordToken>
            {
                new WordToken("word1", 0.5f, 1.0, 2.0, 1),
                new WordToken("word2", 0.5f, 3.0, 4.0, 2)
            });

        // Definieren Sie den erwarteten Untertiteltext
        var expectedSubtitleText = "Ihr erwarteter Untertiteltext";

        // Konfigurieren Sie das Verhalten des Mocks für den Untertitelkonverter
        mockSubtitleConverter
            .Setup(converter => converter.ConvertToWebVttFormat(speechBubble))
            .Returns(expectedSubtitleText);

        // Act (Ausführen)
        await subtitleExporter.ExportSubtitle(speechBubble);

        // Assert (Überprüfen)
        // Überprüfen Sie, ob SendAsync mit den erwarteten Daten aufgerufen wurde
        mockWebSocket.Verify(
            webSocket => webSocket.SendAsync(
                It.Is<ArraySegment<byte>>(buffer => buffer.Array != null && Encoding.UTF8.GetString(buffer.Array) == expectedSubtitleText),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            ),
            Times.Once
        );
    }
}
