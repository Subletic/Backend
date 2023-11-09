using Microsoft.AspNetCore.SignalR;

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
        private readonly FrontendAudioQueueService sendingAudioService;

        /// <summary>
        /// Constructor for Dependency Injection.
        /// </summary>
        public CommunicationHub (FrontendAudioQueueService sendingAudioService)
        {
            this.sendingAudioService = sendingAudioService;
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
                if (sendingAudioService.TryDequeue(out receivedData)) {
                    Console.WriteLine ("Sending audio data to frontend");
                    yield return receivedData!;
                }

                await Task.Delay (200, cancellationToken);
            }
        }
    }
}
