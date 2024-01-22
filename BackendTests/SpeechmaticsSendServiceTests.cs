namespace BackendTests;

using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Backend.Controllers;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;
using Serilog;
using Serilog.Events;

public class SpeechmaticsSendServiceTests
{
    private readonly Mock<ISpeechmaticsConnectionService> mockSpeechmaticsConnectionService = new Mock<ISpeechmaticsConnectionService>();
    private readonly Mock<WebSocket> mockWebSocket = new Mock<WebSocket>();

    private static ILogger logger = new LoggerConfiguration()
        .MinimumLevel.Is(LogEventLevel.Debug)
        .WriteTo.Console()
        .CreateLogger();

    private static IConfiguration configuration = new ConfigurationBuilder()
        .Add(new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("SpeechmaticsConnectionService:SPEECHMATICS_API_URL_AUTHORITY", "dummy"),
                new KeyValuePair<string, string?>("ClientCommunicationSettings:TIMEOUT_IN_SECONDS", "5.0"),
            },
        })
        .Build();

    private static ISpeechmaticsConnectionService realConnectionService = new SpeechmaticsConnectionService(configuration, logger);

    private static byte[] fakeAudioBuffer = new byte[realConnectionService.AudioFormat.GetCheckedSampleRate() * realConnectionService.AudioFormat.GetBytesPerSample()];

    public SpeechmaticsSendServiceTests()
    {
        mockSpeechmaticsConnectionService.Setup(c => c.JsonOptions)
            .Returns(realConnectionService.JsonOptions);
        mockSpeechmaticsConnectionService.Setup(c => c.AudioFormat)
            .Returns(realConnectionService.AudioFormat);
    }

    private static IEnumerable<object[]> exportData()
    {
        var testData = new[]
        {
            // EndOfStreamMessage
            new object[]
            {
                new List<object>
                {
                    new EndOfStreamMessage(0),
                },
                0,
                false,
            },

            // StartRecognitionMessage
            new object[]
            {
                new List<object>
                {
                    new StartRecognitionMessage(
                        audio_format: realConnectionService.AudioFormat,
                        transcription_config: new StartRecognitionMessage_TranscriptionConfig(
                            language: "de",
                            enable_partials: false,
                            additional_vocab: new List<AdditionalVocab>())),
                },
                0,
                false,
            },

            // AudioAddedMessage
            new object[]
            {
                new List<object>
                {
                    fakeAudioBuffer,
                },
                1,
                false,
            },

            // a full communication
            new object[]
            {
                new List<object>
                {
                    new StartRecognitionMessage(
                        audio_format: realConnectionService.AudioFormat,
                        transcription_config: new StartRecognitionMessage_TranscriptionConfig(
                            language: "de",
                            enable_partials: false,
                            additional_vocab: new List<AdditionalVocab>())),
                    fakeAudioBuffer,
                    fakeAudioBuffer,
                    fakeAudioBuffer,
                    fakeAudioBuffer,
                    fakeAudioBuffer,
                    fakeAudioBuffer,
                    fakeAudioBuffer,
                    fakeAudioBuffer,
                    fakeAudioBuffer,
                    fakeAudioBuffer,
                    new EndOfStreamMessage(10),
                },
                10,
                false,
            },
        };

        foreach (object[] test in testData)
            yield return test;
    }

    private static IEnumerable<object> enumerateMessages(object[] messages)
    {
        foreach (object elem in messages)
            yield return elem;
    }

    [Test]
    [TestCaseSource(nameof(exportData))]
    public async Task ReceiveLoop_Messages_OK(
        List<object> messages,
        int expectedSeqNo,
        bool shouldCauseCancel)
    {
        // Arrange
        object[] responseData = messages.ToArray();

        IEnumerator<object> messageEnumerator = enumerateMessages(responseData).GetEnumerator();

        // Use a new element of the messages list, has to be this complex because we have to
        // respond via return & writing to passed buffer
        mockWebSocket.Setup(x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .Callback<ArraySegment<byte>, CancellationToken>(
                (array, ct) =>
                {
                    if (!messageEnumerator.MoveNext())
                        throw new InvalidOperationException("no more message elements");

                    object response = messageEnumerator.Current;
                    string jsonData = JsonSerializer.Serialize(
                        response,
                        response.GetType(),
                        mockSpeechmaticsConnectionService.Object.JsonOptions);

                    logger.Debug($"Mocking receival of data: {jsonData}");
                    byte[] currentJsonData = Encoding.UTF8.GetBytes(jsonData);

                    Buffer.BlockCopy(
                        src: currentJsonData,
                        srcOffset: 0,
                        dst: array.Array!,
                        dstOffset: 0,
                        count: currentJsonData.Length);
                })
            .ReturnsAsync(
                () =>
                {
                    return new WebSocketReceiveResult(
                        Encoding.UTF8.GetBytes(
                            JsonSerializer.Serialize(
                                messageEnumerator.Current,
                                messageEnumerator.Current.GetType(),
                                mockSpeechmaticsConnectionService.Object.JsonOptions)).Length,
                        WebSocketMessageType.Text,
                        true);
                });

        mockSpeechmaticsConnectionService.Setup(c => c.Socket)
            .Returns(mockWebSocket.Object);
        mockSpeechmaticsConnectionService.Setup(c => c.CancellationToken)
            .Returns(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);

        ISpeechmaticsSendService service = new SpeechmaticsSendService(
            mockSpeechmaticsConnectionService.Object,
            logger);

        CancellationTokenSource cts = new CancellationTokenSource();

        // Act
        foreach (object message in messages)
        {
            if (message.GetType() == typeof(byte[]))
            {
                await service.SendAudio((byte[])message);
            }
            else
            {
                // Uses reflection to call generic with variable type argument
                MethodInfo jsonMethod = service.GetType()
                    .GetMethod(nameof(service.SendJsonMessage))!
                    .MakeGenericMethod(new Type[]
                    {
                        message.GetType(),
                    });
                await (Task<bool>)jsonMethod.Invoke(
                    service,
                    new object[]
                    {
                        message,
                    })!;
            }
        }

        // Assert
        Assert.That(service.SequenceNumber, Is.EqualTo(expectedSeqNo));
        /*
        Assert.That(
            mockWordProcessingService.Invocations.Count(
                x => x.Method.Name.Equals(nameof(IWordProcessingService.HandleNewWord))),
            Is.EqualTo(expectedNewWordsCount));
        */
        Assert.That(cts.IsCancellationRequested, Is.EqualTo(shouldCauseCancel));
    }
}
