using Backend.Data;
using Moq;
using System;
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

    [Test]
    // Enqueuing must not throw, must not write to pipe
    public void Enqueue_DoesntWrite()
    {
        Assert.DoesNotThrow (() => _audioQueue.Enqueue(new short[48000]));

        byte[] buf = new byte[2];
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.ThrowsAsync<OperationCanceledException> (async () => await _pipe.Reader.AsStream(true)
            .ReadExactlyAsync(buf, cts.Token));
    }

    [Test]
    // Dequeuing without enqueuing must not throw, must not write data to pipe
    public void Dequeue_DoesntWrite()
    {
        Assert.DoesNotThrow (() => _audioQueue.Dequeue());

        byte[] buf = new byte[2];
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.ThrowsAsync<OperationCanceledException> (async () => await _pipe.Reader.AsStream(true)
            .ReadExactlyAsync(buf, cts.Token));
    }

    [Test]
    // Enqueuing then dequeuing must not throw, must write data to pipe
    public void EnqueueDequeue_Writes()
    {
        _audioQueue.Enqueue (new short[48000]);
        Assert.DoesNotThrow (() => _audioQueue.Dequeue());

        byte[] buf = new byte[2];
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.DoesNotThrowAsync(async () => await _pipe.Reader.AsStream(true)
            .ReadExactlyAsync(buf, cts.Token));
    }

    [Test]
    // Enqueuing then dequeuing must not throw, must write correct data to pipe
    public void EnqueueDequeue_CorrectContent_Single()
    {
        // Prepare
        short[] inBuffer = new short[48000];
        byte[] inBufferBytes = new byte[inBuffer.Length * 2];
        (new Random()).NextBytes (inBufferBytes);
        Buffer.BlockCopy (inBufferBytes, 0, inBuffer, 0, inBufferBytes.Length);

        // Queue / Dequeue
        _audioQueue.Enqueue (inBuffer);
        _audioQueue.Dequeue();

        // Read from Pipe
        byte[] buf = new byte[inBufferBytes.Length];
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.DoesNotThrowAsync(async () => await _pipe.Reader.AsStream(true)
            .ReadExactlyAsync(buf, cts.Token));

        // Check for equality
        Assert.That (inBufferBytes, Is.EqualTo (buf));

        // Check that pipe empty now
        buf = new byte[2];
        cts = new CancellationTokenSource();
        cts.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.ThrowsAsync<OperationCanceledException>(async () => await _pipe.Reader.AsStream(true)
            .ReadExactlyAsync(buf, cts.Token));
    }

    [Test]
    // Multiple enqueues then dequeues must not throw, must write correct data in correct order to pipe
    public void EnqueueDequeue_CorrectContent_Multiple()
    {
        // Prepare
        short[][] buffers = new short[10][];
        byte[][] buffersBytes = new byte[10][];
        for (var i = 0; i < buffers.Length; ++i)
        {
            buffers[i] = new short[48000];
            buffersBytes[i] = new byte[buffers[i].Length * 2];
            (new Random()).NextBytes (buffersBytes[i]);
            Buffer.BlockCopy (buffersBytes[i], 0, buffers[i], 0, buffersBytes[i].Length);
        }

        // Queue
        for (var i = 0; i < buffers.Length; ++i)
        {
            _audioQueue.Enqueue (buffers[i]);
        }

        // TODO Can't call Dequeue more than once without reading from the Pipe inbetween:
        // throws an exception about the reader being closed?
        /*
        // Dequeue
        for (var i = 0; i < buffers.Length; ++i)
        {
            _audioQueue.Dequeue();
        }
        */

        Stream pipeReaderStream = _pipe.Reader.AsStream(true);
        for (var i = 0; i < buffers.Length; ++i)
        {
            // Read from Pipe
            _audioQueue.Dequeue(); // due to above, dequeue only before reading
            byte[] buf = new byte[buffersBytes[i].Length];
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter (TimeSpan.FromSeconds (1));
            Assert.DoesNotThrowAsync(async () => await pipeReaderStream
                .ReadExactlyAsync(buf, cts.Token));

            // Check for equality
            Assert.That (buffersBytes[i], Is.EqualTo (buf));
        }

        // Check that pipe empty now
        byte[] bufFinal = new byte[2];
        CancellationTokenSource ctsFinal = new CancellationTokenSource();
        ctsFinal.CancelAfter (TimeSpan.FromSeconds (1));
        Assert.ThrowsAsync<OperationCanceledException>(async () => await pipeReaderStream
            .ReadExactlyAsync(bufFinal, ctsFinal.Token));
    }
}
