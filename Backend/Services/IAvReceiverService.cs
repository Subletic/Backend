using System.Net.WebSockets;

public interface IAvReceiverService
{
    public Task Start(WebSocket webSocket, CancellationTokenSource ctSource);
}
