using Backend.Data;

using System.Net.WebSockets;
using System.Threading;

namespace Backend.Services;

public interface ISubtitleExporterService
{
    public Task Start(WebSocket webSocket, CancellationTokenSource ctSource);

    public Task ExportSubtitle(SpeechBubble speechBubble);
}
