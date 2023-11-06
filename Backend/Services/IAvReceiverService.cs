using System.Net.WebSockets;

namespace Backend.Services;

public interface IAvReceiverService
{
    public Task Start(WebSocket webSocket, CancellationTokenSource ctSource);
}
