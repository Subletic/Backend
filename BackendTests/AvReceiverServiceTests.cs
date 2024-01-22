namespace BackendTests;

using System.Net.WebSockets;

using Backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;
using Serilog;
using Serilog.Events;

public class AvReceiverServiceTests
{
    private readonly Mock<WebSocket> mockWebSocket = new Mock<WebSocket>();

    private readonly IConfiguration configuration = new ConfigurationBuilder()
        .Add(new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("ClientCommunicationSettings:TIMEOUT_IN_SECONDS", "5.0"),
            },
        })
        .Build();

    private readonly ILogger logger = new LoggerConfiguration()
        .MinimumLevel.Is(LogEventLevel.Debug)
        .WriteTo.Console()
        .CreateLogger();

    private static IEnumerable<byte[]> enumerateBuffers(byte[][] buffers)
    {
        foreach (byte[] elem in buffers)
            yield return elem;
    }

    [Test]
    public async Task Start_PushesAudioCorrectly()
    {
        // Arrange
        byte[][] exampleData = new byte[][]
        {
            new byte[] { 0, 1, 2, 3, },
            new byte[] { 4, 5, 6, 7, },
            new byte[] { 8, 9, },
        };

        IEnumerator<byte[]> bufferEnumerator = enumerateBuffers(exampleData).GetEnumerator();
        bool stillSending = false;

        // Use a new element of the buffers list, has to be this complex because we have to
        // respond via return & writing to passed buffer
        mockWebSocket.Setup(x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .Callback<ArraySegment<byte>, CancellationToken>(
                (array, ct) =>
                {
                    stillSending = bufferEnumerator.MoveNext();

                    if (!stillSending)
                    {
                        logger.Debug($"Mocking close request");
                        return;
                    }

                    logger.Debug($"Mocking receival of buffer");
                    byte[] response = bufferEnumerator.Current;

                    Buffer.BlockCopy(
                        src: response,
                        srcOffset: 0,
                        dst: array.Array!,
                        dstOffset: 0,
                        count: response.Length);
                })
            .ReturnsAsync(
                () =>
                {
                    return new WebSocketReceiveResult(
                        stillSending ? bufferEnumerator.Current.Length : 0,
                        stillSending ? WebSocketMessageType.Binary : WebSocketMessageType.Close,
                        true);
                });

        Mock<IAvProcessingService> mockAvProcessingService = new Mock<IAvProcessingService>();

        IAvReceiverService service = new AvReceiverService(
            mockAvProcessingService.Object,
            configuration,
            logger);

        CancellationTokenSource cts = new CancellationTokenSource();

        // Act
        await service.Start(mockWebSocket.Object, cts);

        // Assert
        Assert.That(
            mockAvProcessingService.Invocations.Count(
                x => x.Method.Name.Equals(nameof(IAvProcessingService.PushProcessedAudio))),
            Is.EqualTo(1));
        Assert.That(cts.IsCancellationRequested, Is.EqualTo(false));
    }

    [Test]
    public async Task Start_TimeoutsCorrectly()
    {
        // Arrange
        byte[][] exampleData = new byte[][]
        {
            new byte[] { 0, 1, 2, 3, },
            new byte[] { 4, 5, 6, 7, },
            new byte[] { 8, 9, },
        };

        IEnumerator<byte[]> bufferEnumerator = enumerateBuffers(exampleData).GetEnumerator();
        bool stillSending = false;

        // Use a new element of the buffers list, has to be this complex because we have to
        // respond via return & writing to passed buffer
        mockWebSocket.Setup(x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .Callback<ArraySegment<byte>, CancellationToken>(
                (array, ct) =>
                {
                    stillSending = bufferEnumerator.MoveNext();

                    if (!stillSending)
                    {
                        logger.Debug($"Mocking timeout");
                        return;
                    }

                    byte[] response = bufferEnumerator.Current;

                    Buffer.BlockCopy(
                        src: response,
                        srcOffset: 0,
                        dst: array.Array!,
                        dstOffset: 0,
                        count: response.Length);
                })
            .ReturnsAsync(
                () =>
                {
                    if (!stillSending)
                        throw new OperationCanceledException("Mocking timeout");

                    return new WebSocketReceiveResult(
                        stillSending ? bufferEnumerator.Current.Length : 0,
                        stillSending ? WebSocketMessageType.Binary : WebSocketMessageType.Close,
                        true);
                });

        Mock<IAvProcessingService> mockAvProcessingService = new Mock<IAvProcessingService>();

        IAvReceiverService service = new AvReceiverService(
            mockAvProcessingService.Object,
            configuration,
            logger);

        CancellationTokenSource cts = new CancellationTokenSource();

        // Act
        await service.Start(mockWebSocket.Object, cts);

        // Assert
        Assert.That(
            mockAvProcessingService.Invocations.Count(
                x => x.Method.Name.Equals(nameof(IAvProcessingService.PushProcessedAudio))),
            Is.EqualTo(1));
        Assert.That(cts.IsCancellationRequested, Is.EqualTo(true));
    }

    [Test]
    public Task Start_HandlesOutsideTimeoutCorrectly()
    {
        // Arrange
        byte[][] exampleData = new byte[][]
        {
            new byte[] { 0, 1, 2, 3, },
            new byte[] { 4, 5, 6, 7, },
            new byte[] { 8, 9, },
        };

        IEnumerator<byte[]> bufferEnumerator = enumerateBuffers(exampleData).GetEnumerator();
        bool stillSending = false;
        CancellationTokenSource cts = new CancellationTokenSource();

        // Use a new element of the buffers list, has to be this complex because we have to
        // respond via return & writing to passed buffer
        mockWebSocket.Setup(x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .Callback<ArraySegment<byte>, CancellationToken>(
                (array, ct) =>
                {
                    stillSending = bufferEnumerator.MoveNext();
                    cts.Cancel();

                    if (!stillSending)
                    {
                        logger.Debug($"Mocking close request");
                        return;
                    }

                    byte[] response = bufferEnumerator.Current;

                    Buffer.BlockCopy(
                        src: response,
                        srcOffset: 0,
                        dst: array.Array!,
                        dstOffset: 0,
                        count: response.Length);
                })
            .ReturnsAsync(
                () =>
                {
                    return new WebSocketReceiveResult(
                        stillSending ? bufferEnumerator.Current.Length : 0,
                        stillSending ? WebSocketMessageType.Binary : WebSocketMessageType.Close,
                        true);
                });

        Mock<IAvProcessingService> mockAvProcessingService = new Mock<IAvProcessingService>();

        IAvReceiverService service = new AvReceiverService(
            mockAvProcessingService.Object,
            configuration,
            logger);

        // Act
        Task receivingTask = service.Start(mockWebSocket.Object, cts);

        // Assert
        Assert.That(
            receivingTask.Wait((int)TimeSpan.FromSeconds(1).TotalMilliseconds),
            Is.EqualTo(true));
        Assert.That(
            mockAvProcessingService.Invocations.Count(
                x => x.Method.Name.Equals(nameof(IAvProcessingService.PushProcessedAudio))),
            Is.EqualTo(1));
        Assert.That(cts.IsCancellationRequested, Is.EqualTo(true));

        return Task.CompletedTask;
    }

    [Test]
    public async Task Start_HandlesExceptionsCorrectly()
    {
        // Arrange
        byte[][] exampleData = new byte[][]
        {
            new byte[] { 0, 1, 2, 3, },
            new byte[] { 4, 5, 6, 7, },
            new byte[] { 8, 9, },
        };

        IEnumerator<byte[]> bufferEnumerator = enumerateBuffers(exampleData).GetEnumerator();
        bool stillSending = false;

        // Use a new element of the buffers list, has to be this complex because we have to
        // respond via return & writing to passed buffer
        mockWebSocket.Setup(x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .Callback<ArraySegment<byte>, CancellationToken>(
                (array, ct) =>
                {
                    stillSending = bufferEnumerator.MoveNext();

                    if (!stillSending)
                    {
                        logger.Debug($"Mocking exception");
                        return;
                    }

                    byte[] response = bufferEnumerator.Current;

                    Buffer.BlockCopy(
                        src: response,
                        srcOffset: 0,
                        dst: array.Array!,
                        dstOffset: 0,
                        count: response.Length);
                })
            .ReturnsAsync(
                () =>
                {
                    if (!stillSending)
                        throw new Exception("Test exception handling");

                    return new WebSocketReceiveResult(
                        stillSending ? bufferEnumerator.Current.Length : 0,
                        stillSending ? WebSocketMessageType.Binary : WebSocketMessageType.Close,
                        true);
                });

        Mock<IAvProcessingService> mockAvProcessingService = new Mock<IAvProcessingService>();

        IAvReceiverService service = new AvReceiverService(
            mockAvProcessingService.Object,
            configuration,
            logger);

        CancellationTokenSource cts = new CancellationTokenSource();

        // Act
        await service.Start(mockWebSocket.Object, cts);

        // Assert
        Assert.That(
            mockAvProcessingService.Invocations.Count(
                x => x.Method.Name.Equals(nameof(IAvProcessingService.PushProcessedAudio))),
            Is.EqualTo(1));
        Assert.That(cts.IsCancellationRequested, Is.EqualTo(true));
    }
}
