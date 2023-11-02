using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Services;

/// <summary>
/// Klasse, die für den Export von Untertiteln über ein WebSocket verantwortlich ist.
/// </summary>
public class SubtitleExporter
{
    private readonly ISubtitleConverter _subtitleConverter; // Eine Abhängigkeit für die Untertitelkonvertierung
    private readonly ClientWebSocket _webSocket; // WebSocket-Instanz für die Kommunikation

    /// <summary>
    /// Initialisiert eine neue Instanz der Klasse SubtitleExporter.
    /// </summary>
    /// <param name="webSocket">Die WebSocket-Instanz, die für den Export verwendet wird.</param>
    /// <param name="subtitleConverter">Der Untertitelkonverter zur Konvertierung von Untertiteln.</param>
    public SubtitleExporter(ClientWebSocket webSocket, ISubtitleConverter subtitleConverter)
    {
        _webSocket = webSocket;
        _subtitleConverter = subtitleConverter;
    }

    /// <summary>
    /// Exportiert einen Untertitel aus einem SpeechBubble.
    /// </summary>
    /// <param name="speechBubble">Der SpeechBubble, der als Untertitel exportiert werden soll.</param>
    public async Task ExportSubtitle(SpeechBubble speechBubble)
    {
        // Konvertiert den SpeechBubble in das WebVTT-Format mithilfe des Untertitelkonverters.
        var subtitleText = _subtitleConverter.ConvertToWebVttFormat(speechBubble);

        // Sendet den Untertitel über das WebSocket.
        await SendSubtitleToWebSocket(subtitleText);
    }

    /// <summary>
    /// Sendet einen Untertitel an das WebSocket.
    /// </summary>
    /// <param name="subtitleText">Der Untertiteltext, der gesendet werden soll.</param>
    private async Task SendSubtitleToWebSocket(string subtitleText)
    {
        try
        {
            // Konvertiert den Untertiteltext in ein Byte-Array und erstellt einen Puffer.
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(subtitleText));

            // Sendet den Puffer über das WebSocket mit einem Nachrichtentyp von "Text".
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch
        {
            // Im Falle eines Fehlers wird eine Fehlermeldung ausgegeben.
            Console.Error.WriteLine("Fehler beim Exportieren des Untertitels");
        }
    }
}
