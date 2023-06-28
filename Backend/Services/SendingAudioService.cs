using System.Collections.Concurrent;

namespace Backend.Services
{
    public class SendingAudioService : ConcurrentQueue<short[]>
    {
/*
        /// <summary>
        /// TODO
        /// </summary>
        public readonly ConcurrentQueue<short[]> SendQueue = new ConcurrentQueue<short[]>();

        /// <summary>
        /// TODO
        /// </summary>
        public void Enqueue (short[] buffer) => SendQueue.Enqueue (buffer);

        /// <summary>
        /// TODO
        /// </summary>
        public short[]? TryDequeue ()
        {
            short[]? buffer;
            bool _ = SendQueue.TryDequeue (out buffer);
            return buffer;
        }
*/
    }
}
