namespace BackendTests;

using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Backend.Controllers;
using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage;
using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.metadata;
using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result;
using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result.alternative;
using Backend.Data.SpeechmaticsMessages.AudioAddedMessage;
using Backend.Data.SpeechmaticsMessages.EndOfTranscriptMessage;
using Backend.Data.SpeechmaticsMessages.ErrorMessage;
using Backend.Data.SpeechmaticsMessages.InfoMessage;
using Backend.Data.SpeechmaticsMessages.RecognitionStartedMessage;
using Backend.Data.SpeechmaticsMessages.RecognitionStartedMessage.language_pack_info;
using Backend.Data.SpeechmaticsMessages.WarningMessage;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;
using Serilog;
using Serilog.Events;

public class SpeechmaticsReceiveServiceTests
{
    private readonly Mock<ISpeechmaticsConnectionService> mockSpeechmaticsConnectionService = new Mock<ISpeechmaticsConnectionService>();
    private readonly Mock<WebSocket> mockWebSocket = new Mock<WebSocket>();

    private readonly ILogger logger = new LoggerConfiguration()
        .MinimumLevel.Is(LogEventLevel.Debug)
        .WriteTo.Console()
        .CreateLogger();

    private readonly IConfiguration configuration = new ConfigurationBuilder()
        .Add(new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("SpeechmaticsConnectionService:SPEECHMATICS_API_URL_AUTHORITY", "dummy"),
                new KeyValuePair<string, string?>("ClientCommunicationSettings:TIMEOUT_IN_SECONDS", "5.0"),
            },
        })
        .Build();

    private static EndOfTranscriptMessage eotMessage = new EndOfTranscriptMessage();

    private static List<AddTranscriptMessage_Result> atMessageResults = new List<AddTranscriptMessage_Result>
    {
        new AddTranscriptMessage_Result(
            type: "word",
            start_time: 0.5,
            end_time: 1.0,
            is_eos: false,
            attaches_to: null,
            alternatives: new List<AddTranscriptMessage_Result_Alternative>
            {
                new AddTranscriptMessage_Result_Alternative(
                    content: "Das",
                    confidence: 0.75,
                    language: "de"),
            }),
        new AddTranscriptMessage_Result(
            type: "word",
            start_time: 1.0,
            end_time: 1.5,
            is_eos: false,
            attaches_to: null,
            alternatives: new List<AddTranscriptMessage_Result_Alternative>
            {
                new AddTranscriptMessage_Result_Alternative(
                    content: "ist",
                    confidence: 0.33,
                    language: "de"),
            }),
        new AddTranscriptMessage_Result(
            type: "word",
            start_time: 1.5,
            end_time: 2.0,
            is_eos: false,
            attaches_to: null,
            alternatives: new List<AddTranscriptMessage_Result_Alternative>
            {
                new AddTranscriptMessage_Result_Alternative(
                    content: "ein",
                    confidence: 1.00,
                    language: "de"),
            }),
        new AddTranscriptMessage_Result(
            type: "word",
            start_time: 2.0,
            end_time: 2.5,
            is_eos: false,
            attaches_to: null,
            alternatives: new List<AddTranscriptMessage_Result_Alternative>
            {
                new AddTranscriptMessage_Result_Alternative(
                    content: "Test",
                    confidence: 0.80,
                    language: "de"),
            }),
        new AddTranscriptMessage_Result(
            type: "punctuation",
            start_time: 2.5,
            end_time: 2.5,
            is_eos: true,
            attaches_to: "previous",
            alternatives: new List<AddTranscriptMessage_Result_Alternative>
            {
                new AddTranscriptMessage_Result_Alternative(
                    content: ".",
                    confidence: 1.00,
                    language: "de"),
            }),
    };

    private static AddTranscriptMessage atMessage = new AddTranscriptMessage(
        format: "2.9",
        metadata: new AddTranscriptMessage_Metadata(
            transcript: "Das ist ein test.",
            start_time: 0.5,
            end_time: 2.5),
        results: atMessageResults);

    public SpeechmaticsReceiveServiceTests()
    {
        mockSpeechmaticsConnectionService.Setup(c => c.JsonOptions)
            .Returns(new SpeechmaticsConnectionService(configuration, logger).JsonOptions);
    }

    private static IEnumerable<object[]> exportData()
    {
        var testData = new[]
        {
            // EndOfTranscriptMessage
            new object[]
            {
                new List<object>
                {
                    eotMessage,
                },
                0,
                0,
                false,
            },

            // InfoMessage
            new object[]
            {
                new List<object>
                {
                    new InfoMessage(
                        code: 123,
                        type: "test1",
                        reason: "for testing 1",
                        quality: "very thorough"),
                    eotMessage,
                },
                0,
                0,
                false,
            },

            // WarningMessage
            new object[]
            {
                new List<object>
                {
                    new WarningMessage(
                        code: 456,
                        type: "test2",
                        reason: "for testing 2",
                        duration_limit: 9999),
                    eotMessage,
                },
                0,
                0,
                false,
            },

            // ErrorMessage
            new object[]
            {
                new List<object>
                {
                    new ErrorMessage(
                        code: 789,
                        type: "test3",
                        reason: "for testing 3"),
                    eotMessage,
                },
                0,
                0,
                true,
            },

            // RecognitionStartedMessage
            new object[]
            {
                new List<object>
                {
                    new RecognitionStartedMessage(
                        id: "{foo-bar-123}",
                        language_pack_info: new RecognitionStartedMessage_LanguagePackInfo(
                            adapted: false,
                            itn: false,
                            language_description: "German",
                            word_delimiter: " ",
                            writing_direction: "left-to-right")),
                    eotMessage,
                },
                0,
                0,
                false,
            },

            // AudioAddedMessage
            new object[]
            {
                new List<object>
                {
                    new AudioAddedMessage(
                        seq_no: 1),
                    eotMessage,
                },
                1,
                0,
                false,
            },

            // AddTranscriptMessage
            new object[]
            {
                new List<object>
                {
                    atMessage,
                    eotMessage,
                },
                0,
                5,
                false,
            },

            // a full communication
            new object[]
            {
                new List<object>
                {
                    new RecognitionStartedMessage(
                        id: "{amazing-unique-identifier}",
                        language_pack_info: new RecognitionStartedMessage_LanguagePackInfo(
                            adapted: false,
                            itn: false,
                            language_description: "German",
                            word_delimiter: " ",
                            writing_direction: "left-to-right")),
                    new InfoMessage(
                        code: 100,
                        type: "hi",
                        reason: "how are you?",
                        quality: "very good"),
                    new WarningMessage(
                        code: 200,
                        type: "hey",
                        reason: "pay attention to this!",
                        duration_limit: 9999),
                    new AudioAddedMessage(
                        seq_no: 1),
                    new AudioAddedMessage(
                        seq_no: 2),
                    new AudioAddedMessage(
                        seq_no: 3),
                    new AudioAddedMessage(
                        seq_no: 4),
                    new AudioAddedMessage(
                        seq_no: 5),
                    new AudioAddedMessage(
                        seq_no: 6),
                    new AudioAddedMessage(
                        seq_no: 7),
                    new AudioAddedMessage(
                        seq_no: 8),
                    new AudioAddedMessage(
                        seq_no: 9),
                    new AudioAddedMessage(
                        seq_no: 10),
                    atMessage,
                    eotMessage,
                },
                10,
                5,
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
        int expectedNewWordsCount,
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

        Mock<IWordProcessingService> mockWordProcessingService = new Mock<IWordProcessingService>();
        Mock<IFrontendCommunicationService> mockFrontendCommunicationService = new Mock<IFrontendCommunicationService>();
        mockSpeechmaticsConnectionService.Setup(c => c.Socket)
            .Returns(mockWebSocket.Object);
        mockSpeechmaticsConnectionService.Setup(c => c.CancellationToken)
            .Returns(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);

        ISpeechmaticsReceiveService service = new SpeechmaticsReceiveService(
            speechmaticsConnectionService: mockSpeechmaticsConnectionService.Object,
            wordProcessingService: mockWordProcessingService.Object,
            frontendCommunicationService: mockFrontendCommunicationService.Object,
            log: logger);

        CancellationTokenSource cts = new CancellationTokenSource();

        // Act
        await service.ReceiveLoop(cts);

        // Assert
        Assert.That(service.SequenceNumber, Is.EqualTo(expectedSeqNo));
        Assert.That(
            mockWordProcessingService.Invocations.Count(
                x => x.Method.Name.Equals(nameof(IWordProcessingService.HandleNewWord))),
            Is.EqualTo(expectedNewWordsCount));
        Assert.That(
            mockFrontendCommunicationService.Invocations.Count(
                x => x.Method.Name.Equals(nameof(IFrontendCommunicationService.AbortCorrection))),
            Is.EqualTo(shouldCauseCancel ? 1 : 0));
        Assert.That(cts.IsCancellationRequested, Is.EqualTo(shouldCauseCancel));
    }
}
