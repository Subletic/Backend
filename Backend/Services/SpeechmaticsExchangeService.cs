namespace Backend.Services;

using System.Net.WebSockets;

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

    private static async Task<bool> dummyTask()
    {
        return await Task.FromResult(true);
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

        if (!keyIsOkay)
            apiKey = oldApiKey;

        // Set up the environment in a correct-looking state for disconnect to work
        sendingTask = dummyTask();
        receivingTask = dummyTask();

        await Disconnect(cts);

        if (keyIsOkay)
            log.Information("Speechmatics API key has been accepted");
        else
            log.Warning("Speechmatics API key has been rejected");

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

    public async Task Disconnect(CancellationTokenSource cts)
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

        log.Information("Disconnecting from Speechmatics");
        await wsClient!.CloseAsync(WebSocketCloseStatus.NormalClosure, closeReason, cts.Token);

        reset();
    }
}
