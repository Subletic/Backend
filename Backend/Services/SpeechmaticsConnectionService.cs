namespace Backend.Services;

using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;

using Serilog;

public partial class SpeechmaticsConnectionService : ISpeechmaticsConnectionService
{
    private const int RECEIVE_BUFFER_SIZE = 1 * 1024;

    /// <summary>
    /// The URL of the Speechmatics RT API.
    /// </summary>
    private static readonly Uri SPEECHMATICS_API_URL = new Uri("wss://neu.rt.speechmatics.com/v2/de");

    /// <summary>
    /// Options to use for all *Message <-> JSON (de)serialisations
    /// </summary>
    /// <see cref="JsonSerializerOptions" />
    private static readonly JsonSerializerOptions JSON_OPTIONS = new()
    {
        IncludeFields = true,
    };

    /// <summary>
    /// A description of the audio format we'll send to the RT API.
    /// <see cref="StartRecognitionMessage_AudioFormat" />
    /// </summary>
    public static readonly StartRecognitionMessage_AudioFormat AUDIO_FORMAT =
        new StartRecognitionMessage_AudioFormat("raw", "pcm_s16le", 48000);

    /// <summary>
    /// The Speechmatics RT API key this instance shall use for the RT transcription.
    /// </summary>
    /// <see cref="Init" />
    private string? apiKey = null;

    private ClientWebSocket? wsClient;

    private CancellationTokenSource cts = new CancellationTokenSource();

    private Serilog.ILogger log;

    public SpeechmaticsConnectionService(Serilog.ILogger log)
    {
        this.log = log;

        reset();
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

    private async Task<bool> checkForResponse()
    {
        try
        {
            CheckConnected();
        }
        catch (InvalidOperationException)
        {
            log.Error("Cannot listen to Speechmatics without a connection");
            return false;
        }

        CancellationTokenSource receiveCts = new CancellationTokenSource();
        byte[] responseBuffer = new byte[4 * 1024]; // only needs to be large enough to hold an Error message

        // The idea here is to listen if Speechmatics sends us a message in response to our connection attempt
        // If it does, it's very likely that it's due to an error
        // Infos on pitting Tasks against each other: https://devblogs.microsoft.com/oldnewthing/20220505-00/?p=106585
        log.Debug("Checking for message from Speechmatics in response to our connection attempt...");
        Task<WebSocketReceiveResult> messageReadingTask = this.Socket.ReceiveAsync(responseBuffer, receiveCts.Token);
        Task<WebSocketReceiveResult> firstFinishedTask = await Task.WhenAny(
            messageReadingTask,
            timeoutTask<WebSocketReceiveResult>(TimeSpan.FromSeconds(1), new WebSocketReceiveResult(0, WebSocketMessageType.Text, true)));

        if (firstFinishedTask == messageReadingTask)
        {
            string responseString = Encoding.UTF8.GetString(responseBuffer, 0, firstFinishedTask.Result.Count);
            log.Error($"Speechmatics unexpectedly sent us a message during our connection test (likely an error): {responseString}");
            return false;
        }

        log.Information("Speechmatics didn't send us anything, assuming everything is fine");

        // clean up task
        receiveCts.Cancel();
        try
        {
            await messageReadingTask;
            log.Warning("Speechmatics response has been received post-cancellation, pretending that we didn't");
        }
        catch (OperationCanceledException)
        {
            log.Debug("Response-awaiting task has been reaped");
        }

        return true;
    }

    public bool Connected
    {
        get
        {
            return wsClient is not null;
        }
    }

    public void CheckConnected()
    {
        if (!this.Connected)
            throw new InvalidOperationException("Not connected to Speechmatics");
    }

    public void CheckDisconnected()
    {
        if (this.Connected)
            throw new InvalidOperationException("Still connected to Speechmatics");
    }

    public ClientWebSocket Socket
    {
        get
        {
            CheckConnected();
            return wsClient!;
        }
    }

    public JsonSerializerOptions JsonOptions
    {
        get
        {
            return JSON_OPTIONS;
        }
    }

    public StartRecognitionMessage_AudioFormat AudioFormat
    {
        get
        {
            return AUDIO_FORMAT;
        }
    }

    public CancellationToken CancellationToken
    {
        get
        {
            return cts.Token;
        }
    }

    private void reset()
    {
        wsClient = null;
        cts.Cancel();
        cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Registers the API key to use with the Speechmatics RT API.
    /// </summary>
    /// <param name="apiKeyVar">Contains the api key to send to Speechmatics.</param>
    /// <returns>True if the envvar was set and Speechmatics accepts its value, false otherwise.</returns>
    public async Task<bool> RegisterApiKey(string apiKeyVar)
    {
        // TODO is it safer to only read a file path to the secret from envvar?
        string? apiKeyEnvMaybe = Environment.GetEnvironmentVariable(apiKeyVar);
        if (apiKeyEnvMaybe == null)
        {
            log.Error($"Requested {apiKeyVar} envvar is not set");
            return false;
        }

        log.Debug($"Found Speechmatics API key: {apiKeyEnvMaybe!}");
        string? oldApiKey = apiKey;
        apiKey = apiKeyEnvMaybe;

        log.Information("Checking if new Speechmatics API key works...");
        CancellationTokenSource cts = new CancellationTokenSource();
        bool keyIsOkay = await Connect(cts.Token);

        // Set up the environment in a correct-looking state for disconnect to work
        if (keyIsOkay)
            keyIsOkay = keyIsOkay && await checkForResponse();

        keyIsOkay = keyIsOkay && await Disconnect(keyIsOkay, cts.Token);

        if (keyIsOkay)
        {
            log.Information("New Speechmatics API key has been accepted");
        }
        else
        {
            log.Warning("New Speechmatics API key has been rejected, keeping previous one");
            apiKey = oldApiKey;
        }

        return keyIsOkay;
    }

    /// <summary>
    /// Open a WebSocket connection to the Speechmatics RT API.
    /// </summary>
    public async Task<bool> Connect(CancellationToken ct)
    {
        if (apiKey is null)
        {
            log.Error($"Valid Speechmatics API key required, call {nameof(RegisterApiKey)} first");
            return false;
        }

        try
        {
            CheckDisconnected();
        }
        catch (InvalidOperationException)
        {
            log.Error("Still connected to Speechmatics, cannot start a new connection");
            return false;
        }

        log.Information($"Connecting to Speechmatics: {SPEECHMATICS_API_URL}");
        ClientWebSocket wsClient = new ClientWebSocket();
        wsClient.Options.SetRequestHeader("Authorization", $"Bearer {apiKey!}");
        await wsClient.ConnectAsync(
            SPEECHMATICS_API_URL,
            ct);

        this.wsClient = wsClient;

        return wsClient.State == WebSocketState.Open;
    }

    public async Task<bool> Disconnect(bool signalSuccess, CancellationToken ct)
    {
        try
        {
            CheckConnected();
        }
        catch (InvalidOperationException)
        {
            log.Error("Not yet connected to Speechmatics, cannot close a current connection");
            return false;
        }

        string closeReason = "Done";
        if (!signalSuccess)
            closeReason = "A problem occurred";

        switch (this.Socket.State)
        {
            case WebSocketState.Connecting:
            case WebSocketState.CloseReceived:
                log.Warning("Speechmatics connection is in an unusual state");
                goto case WebSocketState.Open;

            case WebSocketState.Open:
                log.Information("Disconnecting from Speechmatics");
                await this.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, closeReason, ct);
                break;

            default:
                log.Warning("Speechmatics connection was closed by an unknown reason - "
                    + "this may be indicative of an error, unless we're currently testing a new API key");
                break;
        }

        log.Debug("FIXME: Listen for confirmation of polite close request");

        reset();

        return signalSuccess;
    }

    /*
    private async Task<bool> sendMessages(Stream audioStream, CancellationTokenSource cts)
    {
        // TODO should take transcription_config from ConfigurationService
        bool success = await sendJsonMessage<StartRecognitionMessage>(
            new StartRecognitionMessage(AUDIO_FORMAT),
            cts);
        if (success)
            success = await sendAudio(audioStream, cts);
        if (success)
            success = await sendJsonMessage<EndOfStreamMessage>(new EndOfStreamMessage(1), cts); // FIXME actually confirm seq_no
        return success;
    }

    private async Task<(bool Done, bool Success)> checkForCompletion(Task<bool> taskToCheck)
    {
        TimeSpan checkInterval = TimeSpan.FromSeconds(1);

        bool done = false;
        bool success = true;

        Task<bool> taskSuccess = await Task.WhenAny(
            taskToCheck,
            timeoutTask<bool>(checkInterval, success));
        if (taskSuccess == taskToCheck)
        {
            done = true;
            success = taskSuccess.Result;
        }

        return (done, success);
    }

    public async Task<bool> ManageExchange(Stream audioStream, CancellationTokenSource cts)
    {
        receivingTask = receiveLoop(cts);
        sendingTask = sendMessages(audioStream, cts);

        bool receivingDone = false;
        bool receivingSuccess = true;
        bool sendingDone = false;
        bool sendingSuccess = true;

        do
        {
            if (!receivingDone)
            {
                (bool Done, bool Success) checkResults = await checkForCompletion(receivingTask);
                receivingDone = checkResults.Done;
                receivingSuccess = checkResults.Success;
            }

            if (!sendingDone)
            {
                (bool Done, bool Success) checkResults = await checkForCompletion(sendingTask);
                sendingDone = checkResults.Done;
                sendingSuccess = checkResults.Success;
            }
        }
        while (!receivingDone && !sendingDone);

        return sendingSuccess && receivingSuccess;
    }
    */
}
