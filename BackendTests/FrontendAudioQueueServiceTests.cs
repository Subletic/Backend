namespace BackendTests;

using System;
using Backend.Services;
using Moq;

public class FrontendAudioQueueServiceTests
{
    private readonly FrontendAudioQueueService frontendAudioQueueService;

    public FrontendAudioQueueServiceTests()
    {
        frontendAudioQueueService = new FrontendAudioQueueService();
    }

    [SetUp]
    public void Setup()
    {
        frontendAudioQueueService.Clear();
    }

    [Test]
    public void BufferSize_RejectsWrong()
    {
        short[][] buffers = new short[][]
        {
            Array.Empty<short>(),
            new short[48000 - 1],
            new short[48000 + 1],
            new short[9999999],
        };

        foreach (short[] buffer in buffers)
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => frontendAudioQueueService.Enqueue(buffer));
            Assert.That(ex.Message, Does.Contain("doesn't have correct element count"));
        }
    }

    [Test]
    public void BufferSize_AcceptsCorrect()
    {
        Assert.That(
            () => frontendAudioQueueService.Enqueue(new short[48000]),
            Throws.Nothing);
    }

    [Test]
    public void Queues_CorrectContent_Single()
    {
        short[] inBuffer = new short[48000];
        byte[] inBufferBytes = new byte[inBuffer.Length * 2];
        new Random().NextBytes(inBufferBytes);
        Buffer.BlockCopy(inBufferBytes, 0, inBuffer, 0, inBufferBytes.Length);

        Assert.That(
            () => frontendAudioQueueService.Enqueue(inBuffer),
            Throws.Nothing);

        short[]? outBuffer;
        bool hadQueued = frontendAudioQueueService.TryDequeue(out outBuffer);
        Assert.That(hadQueued, Is.True);
        Assert.That(outBuffer!, Is.EqualTo(inBuffer));
    }

    [Test]
    public void Queues_CorrectContent_Multiple()
    {
        short[][] buffers = new short[10][];
        for (var i = 0; i < buffers.Length; ++i)
        {
            buffers[i] = new short[48000];
            byte[] inBufferBytes = new byte[buffers[i].Length * 2];
            new Random().NextBytes(inBufferBytes);
            Buffer.BlockCopy(inBufferBytes, 0, buffers[i], 0, inBufferBytes.Length);
        }

        for (var i = 0; i < buffers.Length; ++i)
        {
            Assert.That(
                () => frontendAudioQueueService.Enqueue(buffers[i]),
                Throws.Nothing);
        }

        for (var i = 0; i < buffers.Length; ++i)
        {
            short[]? outBuffer;
            bool hadQueued = frontendAudioQueueService.TryDequeue(out outBuffer);
            Assert.That(hadQueued, Is.True);
            Assert.That(outBuffer!, Is.EqualTo(buffers[i]));
        }
    }

    [Test]
    public void ElemCount_TracksCorrectly()
    {
        Assert.That(
            () => frontendAudioQueueService.Count,
            Is.EqualTo(0));

        for (var i = 0; i < 5; ++i)
        {
            frontendAudioQueueService.Enqueue(new short[48000]);
        }

        Assert.That(
            () => frontendAudioQueueService.Count,
            Is.EqualTo(5));

        for (var i = 0; i < 3; ++i)
        {
            short[]? buffer;
            var hadDequeued = frontendAudioQueueService.TryDequeue(out buffer);
        }

        Assert.That(
            () => frontendAudioQueueService.Count,
            Is.EqualTo(2));

        frontendAudioQueueService.Clear();
        Assert.That(
            () => frontendAudioQueueService.Count,
            Is.EqualTo(0));
    }
}
