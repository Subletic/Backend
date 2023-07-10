using System;
using System.Collections.Concurrent;

namespace Backend.Services
{
    /**
      *  <summary>
      *  A ConcurrentQueue<short[]> of audio buffers that needs to be sent to the frontend.
      *
      *  Idea is that <c>AvProcessingService</c> enqueues 1s buffers with decoded raw audio into it, and
      *  <c>CommunicationHub.ReceiveAudioStream</c> tries to periodically dequeue a buffer and send it to the frontend.
      *
      *  Added buffers are checked to have the correct type (signed 16-bit) and size (48000 samples)
      *  for the Frontend to accept.
      *  </summary>
      */
    public class FrontendAudioQueueService : ConcurrentQueue<short[]>
    {
        /**
          *  <summary>
          *  Checks and enqueues the buffer.
          *
          *  <exception cref="ArgumentException">Buffer has wrong amount of samples</exception>
          *  </summary>
          */
        public new void Enqueue (short[] item)
        {
            if (item.Length != 48000) throw new ArgumentException (
                $"Enqueued buffer ({item.Length} samples) doesn't have correct element count (48000 samples)");

            base.Enqueue (item);
        }
    }
}
