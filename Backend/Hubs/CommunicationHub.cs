using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using Backend.Services;

namespace Backend.Hubs
{
    /// <summary>
    /// Used for SignalR communication.
    /// </summary>
    public class CommunicationHub : Hub
    {
        /// <summary>
        /// Dependency Injection of a queue of audio buffers.
        /// <see cref="ReceiveAudioStream" />
        /// </summary>
        private readonly FrontendAudioQueueService _sendingAudioService;

        /// <summary>
        /// Constructor for Dependency Injection.
        /// </summary>
        public CommunicationHub (FrontendAudioQueueService sendingAudioService)
        {
            _sendingAudioService = sendingAudioService;
        }

        /// <summary>
        /// Not actually used.
        /// Should contain Methods which are called from the Frontend.
        /// </summary>
        /// <param name="message"></param>
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", "hallo welt");
        }

        /// <summary>
        /// Frontend subscription to the extracted audio stream.
        ///
        /// Uses <c>FrontendAudioQueueService</c> to receive decoded audio from <c>AvProcessingService</c>.
        /// </summary>
        public async IAsyncEnumerable<short[]> ReceiveAudioStream(
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
        {
            Console.WriteLine ("ReceiveAudioStream started");
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                short[]? receivedData;
                if (_sendingAudioService.TryDequeue(out receivedData)) {
                    Console.WriteLine ("Sending audio data to frontend");
                    yield return receivedData!;
                }

                await Task.Delay (200);
            }
        }
    }
}
