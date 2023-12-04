namespace Backend.Services;

using System.Net.WebSockets;
using System.Text;

using Serilog;

public class SpeechmaticsExchangeService : ISpeechmaticsExchangeService
{
    /// <summary>
    /// The URL of the Speechmatics RT API.
    /// </summary>
    private static readonly Uri speechmaticsApiUrl = new Uri("wss://neu.rt.speechmatics.com/v2/de");

    /// <summary>
    /// The Speechmatics RT API key this instance shall use for the RT transcription.
    /// </summary>
    /// <see cref="Init" />
    private string? apiKey = null;

    private ClientWebSocket? wsClient;

    private Task<bool>? sendingTask;

    private Task<bool>? receivingTask;

    private Serilog.ILogger log;

    public SpeechmaticsExchangeService(Serilog.ILogger log)
    {
        this.log = log;
        reset();
    }

    private static async Task<bool> dummySendingTask()
    {
        return await Task.FromResult(true);
    }

    // https://devblogs.microsoft.com/oldnewthing/20220505-00/?p=106585
    private static async Task<WebSocketReceiveResult> timeoutTask(TimeSpan delay)
    {
        await Task.Delay(delay);
        return new WebSocketReceiveResult(0, WebSocketMessageType.Text, true);
    }

    private async Task<bool> checkForResponse()
    {
        if (!isConnected())
        {
            log.Error("Cannot listen to Speechmatics without a connection");
            return false;
        }

        CancellationTokenSource receiveCts = new CancellationTokenSource();
        byte[] responseBuffer = new byte[4 * 1024]; // only needs to be large enough to hold an Error message

        // The idea here is to listen for abit and see if Speechmatics sends us a message in response to our connection attempt
        // If it does, it's very likely that it's due to an error
        log.Debug("Checking for message from Speechmatics in response to our connection attempt...");
        Task<WebSocketReceiveResult> messageReadingTask = wsClient!.ReceiveAsync(responseBuffer, receiveCts.Token);
        Task<WebSocketReceiveResult> firstFinishedTask = await Task.WhenAny(
            messageReadingTask,
            timeoutTask(TimeSpan.FromSeconds(1)));

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

    private bool isConnected()
    {
        return wsClient is not null;
    }

    private void reset()
    {
        wsClient = null;
        sendingTask = null;
        receivingTask = null;
    }

    /// <summary>
    /// Registers the API key to use with the Speechmatics RT API.
    /// </summary>
    /// <param name="apiKeyVar">Contains the api key to send to Speechmatics.</param>
    /// <returns>True if the envvar was set and Speechmatics accepts its value, false otherwise.</returns>
    public async Task<bool> RegisterApiKey(string apiKeyVar)
    {
        if (isConnected())
            throw new InvalidOperationException("Invalid Speechmatics connection state");

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
        bool keyIsOkay = await Connect(cts);

        // Set up the environment in a correct-looking state for disconnect to work
        sendingTask = dummySendingTask();
        receivingTask = checkForResponse();

        keyIsOkay &= await Disconnect(cts);

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
    public async Task<bool> Connect(CancellationTokenSource cts)
    {
        if (apiKey is null)
        {
            log.Error($"Valid Speechmatics API key required, call {nameof(RegisterApiKey)} first");
            return false;
        }

        if (isConnected())
            throw new InvalidOperationException("Invalid Speechmatics connection state");

        log.Information($"Connecting to Speechmatics: {speechmaticsApiUrl}");
        ClientWebSocket wsClient = new ClientWebSocket();
        wsClient.Options.SetRequestHeader("Authorization", $"Bearer {apiKey!}");
        await wsClient.ConnectAsync(
            speechmaticsApiUrl,
            cts.Token);

        this.wsClient = wsClient;

        return wsClient.State == WebSocketState.Open;
    }

    public async Task<bool> Disconnect(CancellationTokenSource cts)
    {
        if (!isConnected())
            throw new InvalidOperationException("Invalid Speechmatics connection state");

        log.Information("Intending to disconnect from Speechmatics, reaping outstanding tasks...");

        bool sendingSuccess = false;
        if (sendingTask is not null)
            sendingSuccess = await sendingTask;

        bool receivingSuccess = false;
        if (receivingTask is not null)
            receivingSuccess = await receivingTask;

        string closeReason = "Done";
        if (!sendingSuccess || !receivingSuccess)
        {
            closeReason = "A problem occurred while ";

            if (!sendingSuccess)
                closeReason += "sending";
            if (!sendingSuccess && !receivingSuccess)
                closeReason += " and ";
            if (!receivingSuccess)
                closeReason += "receiving";
        }

        switch (wsClient!.State)
        {
            case WebSocketState.Connecting:
            case WebSocketState.CloseReceived:
                log.Warning("Speechmatics connection is in an unusual state");
                goto case WebSocketState.Open;

            case WebSocketState.Open:
                log.Information("Disconnecting from Speechmatics");
                await wsClient!.CloseAsync(WebSocketCloseStatus.NormalClosure, closeReason, cts.Token);
                break;

            default:
                log.Warning("Speechmatics connection was closed by an unknown reason - "
                    + "this may be indicative of an error, unless we're currently testing a new API key");
                break;
        }

        reset();

        return sendingSuccess && receivingSuccess;
    }
}
