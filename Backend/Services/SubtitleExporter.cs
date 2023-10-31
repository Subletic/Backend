using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Services;


public class SubtitleExporter
{
    private readonly ISubtitleConverter _subtitleConverter;
    private readonly ClientWebSocket _webSocket;

    public SubtitleExporter(ClientWebSocket webSocket, ISubtitleConverter subtitleConverter)
    {
        _webSocket = webSocket;
        _subtitleConverter = subtitleConverter;
    }

    public async Task ExportSubtitle(SpeechBubble speechBubble)
    {
        var subtitleText = _subtitleConverter.ConvertToWebVttFormat(speechBubble);
        await SendSubtitleToWebSocket(subtitleText);
    }

    private async Task SendSubtitleToWebSocket(string subtitleText)
    {
        try
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(subtitleText));
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch 
        {
            Console.Error.WriteLine("Error exporting subtitle");
        }
    }
}
