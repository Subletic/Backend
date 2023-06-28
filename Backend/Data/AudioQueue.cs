using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.InteropServices;

namespace Backend.Data;

/**
  *  <summary>
  *  A queue that buffers chunks of audio while they are relevant, and eventually evict them back into a <c>PipeWriter</c> when they are deemed too old.
  *  </summary>
  */
public class AudioQueue
{
    /**
      *  <summary>
      *  A name for this queue, for logging
      *  </summary>
      */
    private string name;

    /**
      *  <summary>
      *  Max queue size. If size would be exceeded, old audio is dequeued into the pipe.
      *  </summary>
      */
    private const int maxQueueCount = 3; // TODO 120 in production

    /**
      *  <summary>
      *  The internal queue that this class wraps, initialised to the maximum size we expect to hold.
      *  </summary>
      */
    private Queue<short[]> audioQueue = new Queue<short[]>(maxQueueCount);

    /**
      *  <summary>
      *  The writing end of a pipe that dequeued audio data will get written to for further processing.
      *
      *  Must be initialised via <c>Init</c>.
      *  </summary>
      */
    private PipeWriter outPipe;

    /**
      *  <summary>
      *  TODO
      *  </summary>
      */
    public AudioQueue (PipeWriter outPipe, string name = "GeneralAudioQueue")
    {
        this.outPipe = outPipe;
        this.name = name;
        audioQueue.Clear();
    }

    // wrapped Queue methods

    /**
      *  <summary>
      *  TODO
      *  </summary>
      */
    public async Task Enqueue(short[] audioBuffer)
    {
        // TODO how do we plan to really handle this? with the timed background service?
        // while queue is deemed full, evict oldest buffers back to pipe
        while (audioQueue.Count >= maxQueueCount)
        {
            await Dequeue();
        }

        audioQueue.Enqueue (audioBuffer);

        Console.WriteLine ($"New audio added to {name}");
    }

    /**
      *  <summary>
      *  TODO
      *  </summary>
      */
    public async Task Dequeue()
    {
        short[] audioBuffer = audioQueue.Dequeue();

        Console.WriteLine ($"Old audio evicted from {name}");

        // TODO inefficient copy of audio data
        byte[] bufferForWriting = new byte[audioBuffer.Length * 2]; // 16-bit short
        Buffer.BlockCopy (audioBuffer, 0, bufferForWriting, 0, bufferForWriting.Length);

        await outPipe.WriteAsync (bufferForWriting);
    }
}
