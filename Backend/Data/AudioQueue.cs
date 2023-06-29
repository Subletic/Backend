using System;
using System.Collections.Generic;
using System.IO.Pipelines;

namespace Backend.Data;

/**
  *  <summary>
  *  A queue that buffers chunks of audio while they are relevant, and eventually evict them
  *  into a supplied <c>PipeWriter</c> when they are deemed too old.
  *  </summary>
  */
public class AudioQueue
{
    /**
      *  <summary>
      *  Max queue size. If size would be exceeded, old audio is dequeued into the pipe.
      *  </summary>
      */
    private const int maxQueueCount = 120;

    /**
      *  <summary>
      *  The internal queue that this class wraps, initialised to the maximum size we expect to hold.
      *  </summary>
      */
    private Queue<short[]> audioQueue = new Queue<short[]>(maxQueueCount);

    /**
      *  <summary>
      *  The writing end of a pipe that dequeued audio data will get written to for further processing.
      *  </summary>
      */
    private PipeWriter outPipe;

    /**
      *  <summary>
      *  Constructs a new audio queue.
      *
      *  <param name="outPipe">The writing side of a pipe where dequeued buffers get written to.</param>
      *  </summary>
      */
    public AudioQueue (PipeWriter outPipe)
    {
        this.outPipe = outPipe;
        audioQueue.Clear();
    }

    // wrapped Queue methods

    /**
      *  <summary>
      *  Enqueue new buffer. If queue is full, <c>Dequeue</c>s old buffers until less full.
      *
      *  <param name="audioBuffer">An audio buffer to add to the queue.</param>
      *  </summary>
      */
    public async Task Enqueue(short[] audioBuffer)
    {
        // TODO how do we plan to really handle this? with the timed background service?
        // while queue is deemed full, evict oldest buffers back to pipe
        while (audioQueue.Count >= maxQueueCount) await Dequeue();

        // FIXME TOC/TOU race, but maxQueueCount is not a critical limit so not really harmful
        audioQueue.Enqueue (audioBuffer);

        Console.WriteLine ($"New audio ({audioBuffer.Length} samples) added to audio queue");
    }

    /**
      *  <summary>
      *  Dequeue oldest buffer and push it into the pipe.
      *
      *  <exception cref="InvalidOperationException">Queue is empty</exception>
      *  </summary>
      */
    public async Task Dequeue()
    {
        short[] audioBuffer = audioQueue.Dequeue();

        Console.WriteLine ($"Old audio ({audioBuffer.Length} samples) evicted from audio queue");

        byte[] bufferForWriting = new byte[audioBuffer.Length * (sizeof (short) / sizeof (byte))];
        Buffer.BlockCopy (audioBuffer, 0, bufferForWriting, 0, bufferForWriting.Length);

        await outPipe.WriteAsync (bufferForWriting);
    }
}
