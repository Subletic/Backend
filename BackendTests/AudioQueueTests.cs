using Backend.Data;
using Moq;
using System;
using System.Diagnostics;
using System.IO.Pipelines;

namespace BackendTests;

public class AudioQueueTests
{
    private readonly Pipe _pipe;

    private AudioQueue _audioQueue;

    public AudioQueueTests()
    {
        _pipe = new Pipe();
        _audioQueue = new AudioQueue(_pipe.Writer);
    }

    [SetUp]
    public void Setup()
    {
        _pipe.Writer.Complete();
        _pipe.Reader.Complete();
        _pipe.Reset();
        // not really designed to be reset / cleared
        _audioQueue = new AudioQueue(_pipe.Writer);
    }

    [Test, Order(1)]
    // Enqueuing must not throw, must not write to pipe
    public void Enqueue_DoesntWrite()
    {
        Assert.DoesNotThrowAsync (async () => await _audioQueue.Enqueue(new short[48000]));

        byte[] buf = new byte[2];
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.ThrowsAsync<OperationCanceledException> (async () => {
            await _pipe.Reader.AsStream(true)
                .ReadExactlyAsync(buf, cts.Token);
        });
    }

    [Test, Order(2)]
    // Dequeuing without enqueuing must throw, must not write data to pipe
    public void Dequeue_DoesntWrite()
    {
        Assert.ThrowsAsync<InvalidOperationException> (async () => await _audioQueue.Dequeue());

        byte[] buf = new byte[2];
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.ThrowsAsync<OperationCanceledException> (async () => {
            await _pipe.Reader.AsStream(true)
                .ReadExactlyAsync(buf, cts.Token);
        });
    }

    [Test, Order(3)]
    // Enqueuing then dequeuing must not throw, must write data to pipe
    public async Task EnqueueDequeue_Writes()
    {
        await _audioQueue.Enqueue (new short[48000]);

        byte[] buf = new byte[2];
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.DoesNotThrowAsync(async () => {
            Task dequeueDone = _audioQueue.Dequeue();
            // test that we can read *something*
            await _pipe.Reader.AsStream(true)
                .ReadExactlyAsync(buf, cts.Token);
            await _pipe.Reader.CompleteAsync(); // throw away rest of data
            await dequeueDone;
        });
    }

    [Test, Order(4)]
    // Enqueuing then dequeuing must not throw, must write correct data to pipe
    public async Task EnqueueDequeue_CorrectContent_Single()
    {
        // Prepare
        short[] inBuffer = new short[48000];
        byte[] inBufferBytes = new byte[inBuffer.Length * 2];
        byte rangeSweep = 0;
        for (var i = 0; i < inBufferBytes.Length; ++i) inBufferBytes[i] = rangeSweep++;
        Buffer.BlockCopy (inBufferBytes, 0, inBuffer, 0, inBufferBytes.Length);

        // Queue / Dequeue
        await _audioQueue.Enqueue (inBuffer);

        // Read from Pipe
        byte[] buf = new byte[inBufferBytes.Length];
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.DoesNotThrowAsync(async () => {
            Task dequeueDone = _audioQueue.Dequeue();
            await _pipe.Reader.AsStream(true)
                .ReadExactlyAsync(buf, cts.Token);
            await dequeueDone;
        });

        // Check for equality
        Assert.That (buf, Is.EqualTo (inBufferBytes));

        // Check that pipe empty now
        buf = new byte[2];
        cts = new CancellationTokenSource();
        cts.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.ThrowsAsync<OperationCanceledException>(async () => {
            await _pipe.Reader.AsStream(true)
                .ReadExactlyAsync(buf, cts.Token);
        });
    }

    [Test]
    // Multiple enqueues then dequeues must not throw, must write correct data in correct order to pipe
    public async Task EnqueueDequeue_CorrectContent_Multiple()
    {
        // Prepare
        short[][] buffers = new short[10][];
        byte[][] buffersBytes = new byte[buffers.Length][];
        byte rangeSweep = 0;
        for (var i = 0; i < buffers.Length; ++i)
        {
            buffers[i] = new short[48000];
            buffersBytes[i] = new byte[buffers[i].Length * 2];
            for (var j = 0; j < buffersBytes.Length; ++j) buffersBytes[i][j] = rangeSweep++;
            Buffer.BlockCopy (buffersBytes[i], 0, buffers[i], 0, buffersBytes[i].Length);
        }

        // Queue
        for (var i = 0; i < buffers.Length; ++i)
        {
            await _audioQueue.Enqueue (buffers[i]);
        }

        // Start dequeuing
        Task doneDequeueing = Task.Run(async () => {
            for (var i = 0; i < buffers.Length; ++i)
            {
                await _audioQueue.Dequeue();
            }
        });

        Stream pipeReaderStream = _pipe.Reader.AsStream(true);
        for (var i = 0; i < buffers.Length; ++i)
        {
            // Read from Pipe
            byte[] buf = new byte[buffersBytes[i].Length];
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter (TimeSpan.FromSeconds (1));
            Assert.DoesNotThrowAsync(async () => {
                await pipeReaderStream
                    .ReadExactlyAsync(buf, cts.Token);
            });

            // Check for equality
            Assert.That (buf, Is.EqualTo (buffersBytes[i]));
        }

        // Await end of dequeuing
        await doneDequeueing;

        // Check that pipe empty now
        byte[] bufFinal = new byte[2];
        CancellationTokenSource ctsFinal = new CancellationTokenSource();
        ctsFinal.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.ThrowsAsync<OperationCanceledException>(async () => {
            await pipeReaderStream
                .ReadExactlyAsync(bufFinal, ctsFinal.Token);
        });
    }

    [Test]
    // Enqueues past the queue limit should trigger a dequeue of the oldest second. Order should be preserved.
    public async Task EnqueueLimit_TriggersDequeueOfOldest()
    {
        short[] bufferMarks = {
            0,
            short.MaxValue,
            short.MinValue,
            0x2205,
            -0x1610,
        };

        short[][] buffers = new short[bufferMarks.Length][];
        byte[][] bufferMarksBytes = new byte[bufferMarks.Length][];
        for (var i = 0; i < bufferMarks.Length; ++i)
        {
            buffers[i] = new short[48000];
            buffers[i][0] = bufferMarks[i];
            bufferMarksBytes[i] = new byte[sizeof (short)];
            Buffer.BlockCopy (bufferMarks, i * sizeof (short), bufferMarksBytes[i], 0, bufferMarksBytes[i].Length);
        }

        short[] otherBuffer = new short[48000];

        for (var i = 0; i < buffers.Length; ++i)
        {
            await _audioQueue.Enqueue (buffers[i]);
        }

        // TODO query AudioQueue for its max size instead of assuming 2 minutes
        for (var i = 0; i < (2 * 60) - buffers.Length; ++i)
        {
            await _audioQueue.Enqueue (otherBuffer);
        }

        Stream pipeReaderStream = _pipe.Reader.AsStream(true);
        for (var i = 0; i < buffers.Length; ++i)
        {
            Task enqueuingDone = _audioQueue.Enqueue (otherBuffer);
            byte[] buf = new byte[buffers[i].Length * 2];
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter (TimeSpan.FromSeconds (1));
            Assert.DoesNotThrowAsync(async () => {
                await pipeReaderStream
                    .ReadExactlyAsync(buf, cts.Token);
            });

            byte[] bufMark = new byte[sizeof (short)];
            Array.Copy (buf, 0, bufMark, 0, bufMark.Length);
            Assert.That (bufMark, Is.EqualTo (bufferMarksBytes[i]));
        }
    }
}
