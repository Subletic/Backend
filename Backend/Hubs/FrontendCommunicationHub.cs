namespace Backend.Hubs;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.SignalR;
using Serilog;

/// <summary>
/// SignalR Hub class used for real-time communication with the frontend.
/// It provides functionalities for audio streaming and managing speech bubbles.
/// </summary>
public class FrontendCommunicationHub : Hub
{
    /// <summary>
    /// Service for handling audio data and speech bubble operations.
    /// </summary>
    private readonly IFrontendCommunicationService frontendCommunicationService;

    /// <summary>
    /// Logger for logging information and errors within this class.
    /// </summary>
    private readonly Serilog.ILogger logger;

    /// <summary>
    /// Constructor for the FrontendProviderHub.
    /// Initializes a new instance with injected dependencies.
    /// </summary>
    /// <param name="frontendCommunicationService">Service to manage audio data and speech bubbles.</param>
    /// <param name="logger">Logger for logging activities.</param>
    public FrontendCommunicationHub(IFrontendCommunicationService frontendCommunicationService, Serilog.ILogger logger)
    {
        this.frontendCommunicationService = frontendCommunicationService;
        this.logger = logger;
    }

    /// <summary>
    /// Initiates audio streaming to the client that called this method.
    /// Streams audio data as it becomes available.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>An asynchronous enumerable of audio data chunks.</returns>
    public async IAsyncEnumerable<short[]> ReceiveAudioStream([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            short[]? audioData;
            if (frontendCommunicationService.TryDequeue(out audioData))
            {
                if (audioData != null)
                {
                    yield return audioData;
                }
            }

            await Task.Delay(200, cancellationToken);
        }
    }
}
