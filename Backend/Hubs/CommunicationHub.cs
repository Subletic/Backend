using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

using Backend.Services;

namespace Backend.Hubs
{
    /// <summary>
    /// Used for SignalR communication.
    /// </summary>
    public class CommunicationHub : Hub
    {
        /// TODO
        private readonly SendingAudioService _sendingAudioService;

        /// TODO
        public CommunicationHub (SendingAudioService sendingAudioService)
        {
            _sendingAudioService = sendingAudioService;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static readonly ConcurrentQueue<byte[]> SendQueue = new ConcurrentQueue<byte[]>();

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
                    Console.WriteLine ("Sending data");
                    yield return receivedData!;
                }

                await Task.Delay (100);
            }
            Console.WriteLine ("ReceiveAudioStream done");
        }
    }
}
