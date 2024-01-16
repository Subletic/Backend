namespace Backend.Services;

using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage;
using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result;
using Backend.Data.SpeechmaticsMessages.AudioAddedMessage;
using Backend.Data.SpeechmaticsMessages.EndOfTranscriptMessage;
using Backend.Data.SpeechmaticsMessages.ErrorMessage;
using Backend.Data.SpeechmaticsMessages.InfoMessage;
using Backend.Data.SpeechmaticsMessages.WarningMessage;
using ILogger = Serilog.ILogger;

/// <summary>
/// Service that handles the receiving of messages from Speechmatics.
/// </summary>
public partial class SpeechmaticsReceiveService : ISpeechmaticsReceiveService
{
    private const int RECEIVE_BUFFER_SIZE = 1024;

    /// <summary>
    /// A regular expression to extract the type of message Speechmatics sent us.
    /// Based on the type string, the message will get deserialised to an object
    /// of the corresponding message class.
    /// Matches: <c>"message" : "(SomeMessageType)"</c>
    /// </summary>
    [GeneratedRegex(@"""message""\s*:\s*""([^""]+)""")]
    private static partial Regex MESSAGE_TYPE_REGEX();

    private ISpeechmaticsConnectionService speechmaticsConnectionService;

    private IWordProcessingService wordProcessingService;

    private ILogger log;

    /// <summary>
    /// Gets the sequence number of the last audio chunk that was sent to Speechmatics.
    /// </summary>
    public ulong SequenceNumber
    {
        get;
        private set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechmaticsReceiveService"/> class.
    /// </summary>
    /// <param name="speechmaticsConnectionService">Service that handles the connection to Speechmatics.</param>
    /// <param name="wordProcessingService">Service that handles the processing of words into our system.</param>
    /// <param name="log">The logger.</param>
    public SpeechmaticsReceiveService(
        ISpeechmaticsConnectionService speechmaticsConnectionService,
        IWordProcessingService wordProcessingService,
        ILogger log)
    {
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.wordProcessingService = wordProcessingService;
        this.log = log;

        resetSequenceNumber();
    }

    private void resetSequenceNumber()
    {
        SequenceNumber = 0;
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
        speechmaticsConnectionService.ThrowIfNotConnected();

        byte[] chunkBuffer = new byte[RECEIVE_BUFFER_SIZE];
        List<byte> messageChunks = new List<byte>();
        bool completed = false;

        do
        {
            log.Debug("Listening for data from Speechmatics");
            WebSocketReceiveResult response = await speechmaticsConnectionService.Socket.ReceiveAsync(
                buffer: chunkBuffer,
                cancellationToken: speechmaticsConnectionService.CancellationToken);
            log.Debug("Received data from Speechmatics");

            // FIXME AddRange'ing a Span directly for better performance is a .NET 8 feature
            byte[] bufferToAdd = chunkBuffer;
            if (response.Count != chunkBuffer.Length)
            {
                bufferToAdd = new byte[response.Count];
                Array.Copy(chunkBuffer, bufferToAdd, response.Count);
            }

            messageChunks.AddRange(bufferToAdd);
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
        log.Debug($"Found {messageMatches.Count} matches of message type pattern in data");
        if (messageMatches.Count != 1)
            throw new ArgumentException($"Found unexpected amount of message type matches: {messageMatches.Count}");

        string messageName = messageMatches[0].Groups[1].ToString() + "Message";
        Type? messageType = Type.GetType($"Backend.Data.SpeechmaticsMessages.{messageName}.{messageName}");
        if (messageType is null)
        {
            string errorMsg = $"Found unknown message type: {messageName}";
            log.Error(errorMsg);
            throw new ArgumentException(errorMsg);
        }

        log.Information($"Received {messageName} object from Speechmatics");
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
                bindingAttr: BindingFlags.Public | BindingFlags.Static,
                binder: null,
                callConvention: CallingConventions.Standard,
                types: new Type[]
                {
                    typeof(string),
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
                Encoding.UTF8.GetString(completeMessage),
                speechmaticsConnectionService.JsonOptions,
            })!;
    }

    private async Task handleAddTranscriptMessage(AddTranscriptMessage message)
    {
        foreach (AddTranscriptMessage_Result transcript in message.results!)
        {
            // the specs say an AddTranscript.results may come without an alternatives list.
            // TODO what is its purpose?
            if (transcript.alternatives is null)
            {
                throw new InvalidOperationException(
                    "Received a transcript result without an alternatives list. "
                    + "Specifications say this is a possibility, but don't know what to do with it?");
            }

            await wordProcessingService.HandleNewWord(new WordToken(
                transcript.alternatives![0].content, // docs say this sends a list, I've only ever seen it send 1 result
                (float)transcript.alternatives![0].confidence,
                transcript.start_time,
                transcript.end_time,
                1));

            // TODO api can send a string if this feature is requested, extract a number from it
            // https://docs.speechmatics.com/features/diarization#speaker-diarization
            // speaker identified: "S<speaker-id>"
            // not identified: "UU"
        }
    }

    private void handleAudioAddedMessage(AudioAddedMessage message)
    {
        if (message.seq_no != (SequenceNumber + 1))
        {
            string errorMsg = $"Sequence number mismatch: Expected next-in-line {SequenceNumber + 1}, received {message.seq_no}";
            log.Error(errorMsg);
            throw new ArgumentException(errorMsg);
        }

        SequenceNumber += 1;
    }

    private async Task handleEndOfTranscriptMessage(EndOfTranscriptMessage message)
    {
        Task flushBufferTask = wordProcessingService.FlushBufferToNewSpeechBubble();
        log.Information("Transcription is over");
        await flushBufferTask;
    }

    private void handleErrorMessage(ErrorMessage message)
    {
        string errorString = $"Speechmatics reports a fatal error: {message.type}: {message.reason}";
        log.Error(errorString);
        throw new Exception(errorString);
    }

    private void handleInfoMessage(InfoMessage message)
    {
        log.Information($"Speechmatics reports some information: {message.type}: {message.reason}");
    }

    private void handleWarningMessage(WarningMessage message)
    {
        log.Warning($"Speechmatics reports a non-critical warning: {message.type}: {message.reason}");
    }

    private async Task<bool> processMessage(dynamic messageObject, Type messageType)
    {
        // TODO: Can we somehow use compile-time class names in cases to avoid copy-pasting them here?
        switch (messageType.Name)
        {
            case "AudioAddedMessage":
                handleAudioAddedMessage((AudioAddedMessage)messageObject);
                return false;

            case "AddTranscriptMessage":
                await handleAddTranscriptMessage((AddTranscriptMessage)messageObject);
                return false;

            case "EndOfTranscriptMessage":
                await handleEndOfTranscriptMessage((EndOfTranscriptMessage)messageObject);
                return true;

            case "ErrorMessage":
                handleErrorMessage((ErrorMessage)messageObject);
                return true; // should have thrown, but all branches are required to break or return

            case "InfoMessage":
                handleInfoMessage((InfoMessage)messageObject);
                return false;

            case "WarningMessage":
                handleWarningMessage((WarningMessage)messageObject);
                return false;

            default:
                log.Information("Nothing we need to do / know how to handle about that message.");
                return false;
        }
    }

    /// <summary>
    /// The main loop that receives messages from Speechmatics and processes them.
    /// </summary>
    /// <param name="ctSource">Reference to to CT Source to cancel all other services in case something goes wrong</param>
    /// <returns>A <c>Task</c> that can be <c>await</c>ed to wait for the loop to finish.</returns>
    public async Task<bool> ReceiveLoop(CancellationTokenSource ctSource)
    {
        bool done = false;
        bool success = true;
        resetSequenceNumber();

        do
        {
            try
            {
                byte[] messageBuffer = await receiveJsonResponse();
                Type messageType = identifyMessage(messageBuffer);
                dynamic message = deserialiseMessage(messageBuffer, messageType);
                done = await processMessage(message, messageType);
            }
            catch (Exception e)
            {
                log.Error($"Exception occured while trying to receive data from Speechmatics: {e.Message}");
                log.Debug(e.ToString());
                ctSource.Cancel();
                done = true;
                success = false;
            }
        }
        while (!done);

        return success;
    }

    /// <summary>
    /// Test the deserialisation of a message.
    /// </summary>
    public void TestDeserialisation()
    {
        findDeserialiserMethod(typeof(InfoMessage));
    }
}
