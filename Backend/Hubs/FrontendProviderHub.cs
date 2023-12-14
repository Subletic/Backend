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
/// Used for SignalR communication.
/// </summary>
public class FrontendProviderHub : Hub
{
    /// <summary>
    /// Dependency Injection of a queue of audio buffers.
    /// <see cref="ReceiveAudioStream" />
    /// </summary>
    private readonly FrontendAudioQueueService sendingAudioService;

    /// <summary>
    /// Logger for logging within this class
    /// </summary>
    private readonly Serilog.ILogger logger;

    /// <summary>
    /// Constructor for Dependency Injection.
    /// </summary>
    /// <param name="sendingAudioService">The sending audio service.</param>
    /// <param name="logger">for logging </param>
    public FrontendProviderHub(FrontendAudioQueueService sendingAudioService, Serilog.ILogger logger)
    {
        this.sendingAudioService = sendingAudioService;
        this.logger = logger;
    }

    /// <summary>
    /// Frontend subscription to the extracted audio stream.
    ///
    /// Uses <c>FrontendAudioQueueService</c> to receive decoded audio from <c>AvProcessingService</c>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Audio stream</returns>
    public async IAsyncEnumerable<short[]> SubscribeToAudioStream([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        logger.Information("ReceiveAudioStream started");
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            short[]? receivedData;
            if (sendingAudioService.TryDequeue(out receivedData))
            {
                logger.Information("Sending audio data to frontend");
                yield return receivedData!;
            }

            await Task.Delay(200, cancellationToken);
        }
    }
}
