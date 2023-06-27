using Microsoft.AspNetCore.SignalR;

using System.Runtime.CompilerServices;

namespace Backend.Hubs
{
    /// <summary>
    /// Used for SignalR communication.
    /// </summary>
    public class CommunicationHub : Hub
    {
        /// <summary>
        /// Not actually used.
        /// Should contain Methods which are called from the Frontend.
        /// </summary>
        /// <param name="message"></param>
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", "hallo welt");
        }

        static private IEnumerable<short> genExampleData(ulong hz, double freq)
        {
            double doublePiFreq = 2 * Math.PI * freq;
            double amp = 0.75d;
            for (ulong step = 0; step < hz; ++step)
            {
                yield return (short) (short.MaxValue * (
                    amp * Math.Sin (doublePiFreq * ((double)step / (double)hz))));
            }
        }

        /// <summary>
        /// Frontend subscription to the extracted audio stream.
        /// </summary>
        public async IAsyncEnumerable<short[]> ReceiveAudioStream(
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
        {
            // TODO "subscribe" to the audio buffer, send whenever there's new audio
            // currently generating & sending dummy data (440Hz sine)
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // some signal from the audio queue to know when new data is available
                short[] outData = new short[48000];
                ulong i = 0;
                foreach (short volValue in genExampleData (48000, 440.0d))
                {
                    outData[i++] = volValue;
                }
                yield return outData;

                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
