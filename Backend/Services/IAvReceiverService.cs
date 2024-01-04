namespace Backend.Services;

using System.Net.WebSockets;

/// <summary>
/// Interface for a service that handles communication with an AV receiver.
/// </summary>
public interface IAvReceiverService
{
    /// <summary>
    /// Starts the service with the given WebSocket and CancellationTokenSource.
    /// </summary>
    /// <param name="webSocket">The WebSocket to use for communication.</param>
    /// <param name="ctSource">The CancellationTokenSource to use for cancellation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task<bool> Start(WebSocket webSocket, CancellationTokenSource ctSource);
}
