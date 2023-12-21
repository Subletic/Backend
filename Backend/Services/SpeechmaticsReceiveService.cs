namespace Backend.Services;

using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage;
using Backend.Data.SpeechmaticsMessages.AudioAddedMessage;
using Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;
using Backend.Data.SpeechmaticsMessages.EndOfTranscriptMessage;
using Backend.Data.SpeechmaticsMessages.ErrorMessage;
using Backend.Data.SpeechmaticsMessages.InfoMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage;

using Serilog;

public partial class SpeechmaticsReceiveService : ISpeechmaticsReceiveService
{
    private const int RECEIVE_BUFFER_SIZE = 1 * 1024;

    /// <summary>
    /// A regular expression to extract the type of message Speechmatics sent us.
    /// Based on the type string, the message will get deserialised to an object
    /// of the corresponding message class.
    /// Matches: <c>"message" : "(SomeMessageType)"</c>
    /// </summary>
    [GeneratedRegex(@"""message""\s*:\s*""([^""]+)""")]
    private static partial Regex MESSAGE_TYPE_REGEX();

    private ISpeechmaticsConnectionService speechmaticsConnectionService;

    private Serilog.ILogger log;

    public SpeechmaticsReceiveService(ISpeechmaticsConnectionService speechmaticsConnectionService, Serilog.ILogger log)
    {
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.log = log;
    }

    /// <summary>
    /// A Task that waits a specified amount of time before returning a dummy <c>WebSocketReceiveResult</c>
    /// for timeout purposes.
    /// </summary>
    private static async Task<T> timeoutTask<T>(TimeSpan delay, T timeoutResponse)
    {
        await Task.Delay(delay);
        return timeoutResponse;
    }

    private async Task<byte[]> receiveJsonResponse()
    {
        speechmaticsConnectionService.CheckConnected();

        byte[] chunkBuffer = new byte[RECEIVE_BUFFER_SIZE];
        List<byte> messageChunks = new List<byte>(RECEIVE_BUFFER_SIZE);
        bool completed = false;
        do
        {
            WebSocketReceiveResult response = await speechmaticsConnectionService.Socket.ReceiveAsync(
                buffer: chunkBuffer,
                cancellationToken: speechmaticsConnectionService.CancellationToken);

            messageChunks.AddRange(chunkBuffer);
            completed = response.EndOfMessage;
        }
        while (!completed);
        byte[] completeMessage = messageChunks.ToArray();

        log.Debug($"Received message: {Encoding.UTF8.GetString(completeMessage)}");
        return completeMessage;
    }

    private Type identifyMessage(byte[] messageBuffer)
    {
        MatchCollection messageMatches = MESSAGE_TYPE_REGEX().Matches(Encoding.UTF8.GetString(messageBuffer));
        if (messageMatches.Count != 1)
            throw new ArgumentException($"Found unexpected amount of message type matches: {messageMatches.Count}");

        string messageName = messageMatches[0].Groups[1].ToString() + "Message";
        Type? messageType = Type.GetType($"Backend.Data.SpeechmaticsMessages.{messageName}, {messageName}");
        if (messageType is null)
            throw new ArgumentException($"Found unknown message type: {messageName}");

        log.Information($"Received {messageName} message from Speechmatics");
        return messageType!;
    }

    /// <summary>
    /// Use fancy reflection to find a JSON deserialiser for a type in a generic manner.
    /// Finds: <c>public static T JsonSerializer.Deserialize<T>(byte[], JsonSerializerOptions)</c>
    /// </summary>
    private MethodInfo findDeserialiserMethod(Type messageType)
    {
        return typeof(JsonSerializer)
            .GetMethod(
                name: nameof(JsonSerializer.Deserialize),
                genericParameterCount: 1,
                bindingAttr: BindingFlags.Public | BindingFlags.Static /*| BindingFlags.InvokeMethod*/,
                binder: null,
                callConvention: CallingConventions.Standard,
                types: new Type[]
                {
                    typeof(ReadOnlySpan<byte>),
                    typeof(JsonSerializerOptions),
                },
                modifiers: null)!
            .MakeGenericMethod(
                new Type[] { messageType });
    }

    private dynamic deserialiseMessage(byte[] completeMessage, Type messageType)
    {
        // Cannot get statically-typed return, since type is variable and dynamically-determined
        return findDeserialiserMethod(messageType).Invoke(
            obj: null, // static method call, no instance
            parameters: new object[]
            {
                completeMessage,
                speechmaticsConnectionService.JsonOptions,
            })!;
    }

    private void handleAudioAdded(AudioAddedMessage message)
    {
        log.Error("FIXME: Check returned seq_no");
    }

    private void handleAddTranscript(AddTranscriptMessage message)
    {
        log.Error("FIXME: Digest returned transcript");
    }

    private bool processMessage(dynamic messageObject, Type messageType)
    {
        // TODO: Can we somehow use compile-time class names in cases to avoid copy-pasting them here?
        switch (messageType.Name)
        {
            case "AudioAddedMessage":
                handleAudioAdded((AudioAddedMessage)messageObject);
                return false;

            case "AddTranscriptMessage":
                handleAddTranscript((AddTranscriptMessage)messageObject);
                return false;

            case "EndOfTranscriptMessage":
                log.Information("Transcription is over");
                return true;

            case "ErrorMessage":
                ErrorMessage errorMessage = (ErrorMessage)messageObject;
                {
                    string errorString = $"Speechmatics reports a fatal error: {errorMessage.type}: {errorMessage.reason}";
                    log.Error(errorString);
                    throw new Exception(errorString);
                }

            default:
                log.Information("Nothing we need to / know how to handle.");
                return false;
        }
    }

    public async Task<bool> ReceiveLoop()
    {
        bool done = false;
        do
        {
            byte[] messageBuffer = await receiveJsonResponse();
            Type messageType = identifyMessage(messageBuffer);
            dynamic message = deserialiseMessage(messageBuffer, messageType);
            done = processMessage(message, messageType);
        }
        while (!done);

        // FIXME return failure/success indication (try-catch to handle comm errors & ErrorMessage)
        return true;
    }

    public void TestDeserialisation()
    {
        findDeserialiserMethod(typeof(InfoMessage));
    }
}
