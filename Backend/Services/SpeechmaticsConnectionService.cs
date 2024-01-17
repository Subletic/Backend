namespace Backend.Services;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;
using ILogger = Serilog.ILogger;

/// <summary>
/// A service that handles a WebSocket connection to Speechmatics
/// </summary>
public class SpeechmaticsConnectionService : ISpeechmaticsConnectionService
{
    private const int RECEIVE_BUFFER_SIZE = 4 * 1024;

    /// <summary>
    /// A format template for the URL through which we'll connect to the Speechmatics RT API.
    /// Note that this cannot be fully user-controlled due to various assumptions about
    /// what API version we're talking against, and what type of data we're expected to be sending.
    /// Currently, only the authority part of the URL makes sense to be exchangable.
    /// See the configuration option <c>SpeechmaticsConnectionService:SPEECHMATICS_API_URL_AUTHORITY</c>.
    /// </summary>
    private const string SPEECHMATICS_API_URL_TEMPLATE = "wss://{0}/v2/de";

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

    private readonly IConfiguration configuration;

    private readonly ILogger log;

    private readonly TimeSpan connectionTimeout;

    /// <summary>
    /// The Speechmatics RT API key this instance shall use for the RT transcription.
    /// </summary>
    /// <see cref="Init" />
    private string? apiKey = null;

    private Uri speechmaticsApiUrl;

    private ClientWebSocket? wsClient = null;

    private CancellationTokenSource connectionLifetimeToken = new CancellationTokenSource();

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechmaticsConnectionService"/> class.
    /// </summary>
    /// <param name="configuration">DI appsettings reference</param>
    /// <param name="log">The logger</param>
    public SpeechmaticsConnectionService(IConfiguration configuration, ILogger log)
    {
        this.configuration = configuration;
        this.log = log;
        speechmaticsApiUrl = new Uri(string.Format(
            SPEECHMATICS_API_URL_TEMPLATE,
            configuration.GetValue<string>("SpeechmaticsConnectionService:SPEECHMATICS_API_URL_AUTHORITY")));
        connectionLifetimeToken.Cancel();
        connectionTimeout =
            TimeSpan.FromSeconds(configuration.GetValue<double>("ClientCommunicationSettings:TIMEOUT_IN_SECONDS"));
    }

    private CancellationToken makeConnectionTimeoutToken()
    {
        return new CancellationTokenSource((int)connectionTimeout.TotalMilliseconds).Token;
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
        if (Connected != true)
        {
            log.Error("Cannot listen to Speechmatics without a connection");
            return false;
        }

        CancellationTokenSource receiveCts = new CancellationTokenSource();
        byte[] responseBuffer = new byte[RECEIVE_BUFFER_SIZE]; // only needs to be large enough to hold an Error message

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

    /// <summary>
    /// Gets a value indicating whether we're currently connected to Speechmatics.
    /// </summary>
    public bool Connected
    {
        get
        {
            return wsClient is not null;
        }
    }

    /// <summary>
    /// Throws an exception if we're not currently connected to Speechmatics.
    /// </summary>
    /// <exception cref="InvalidOperationException">If we're not connected to Speechmatics</exception>
    public void ThrowIfNotConnected()
    {
        if (Connected != true)
            throw new InvalidOperationException("Not connected to Speechmatics");
    }

    /// <summary>
    /// Gets the WebSocket to Speechmatics.
    /// </summary>
    public WebSocket Socket
    {
        get
        {
            ThrowIfNotConnected();
            return wsClient!;
        }
    }

    /// <summary>
    /// Gets the common (de)serialiser options to use for this connection
    /// </summary>
    public JsonSerializerOptions JsonOptions
    {
        get
        {
            return JSON_OPTIONS;
        }
    }

    /// <summary>
    /// Gets what audio format will be sent over the connection.
    /// </summary>
    public StartRecognitionMessage_AudioFormat AudioFormat
    {
        get
        {
            return AUDIO_FORMAT;
        }
    }

    /// <summary>
    /// Gets a <c>CancellationToken</c> corresponding to the existence of this connection.
    /// </summary>
    public CancellationToken CancellationToken
    {
        get
        {
            return CancellationTokenSource.CreateLinkedTokenSource(
                token1: connectionLifetimeToken.Token,
                token2: makeConnectionTimeoutToken()).Token;
        }
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
        CancellationTokenSource connectionLifetimeToken = new CancellationTokenSource();
        bool keyIsOkay = await Connect(connectionLifetimeToken.Token);

        // Set up the environment in a correct-looking state for disconnect to work
        if (keyIsOkay)
            keyIsOkay = keyIsOkay && await checkForResponse();

        keyIsOkay = keyIsOkay && await Disconnect(keyIsOkay, connectionLifetimeToken.Token);

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
    /// <param name="ct">A CancellationToken to use for the network calls</param>
    /// <returns>Whether or not everything went well</returns>
    public async Task<bool> Connect(CancellationToken ct)
    {
        if (apiKey is null)
        {
            log.Error($"Valid Speechmatics API key required, call {nameof(RegisterApiKey)} first");
            return false;
        }

        if (Connected != false)
        {
            log.Error("Still connected to Speechmatics, cannot start a new connection");
            return false;
        }

        log.Information($"Connecting to Speechmatics: {speechmaticsApiUrl}");
        ClientWebSocket wsClient = new ClientWebSocket();
        wsClient.Options.SetRequestHeader("Authorization", $"Bearer {apiKey!}");
        try
        {
            await wsClient.ConnectAsync(
                speechmaticsApiUrl,
                ct);
        }
        catch (Exception e)
        {
            log.Error($"Failed to connect to Speechmatics: {e.Message}");
            return false;
        }

        this.wsClient = wsClient;
        connectionLifetimeToken = new CancellationTokenSource();

        return wsClient.State == WebSocketState.Open;
    }

    /// <summary>
    /// Close the WebSocket connection to the Speechmatics RT API.
    /// </summary>
    /// <param name="signalSuccess">Whether we should tell Speechmatics that everything went well</param>
    /// <param name="ct">A CancellationToken to use for the network calls</param>
    /// <returns>Whether or not everything went well</returns>
    public async Task<bool> Disconnect(bool signalSuccess, CancellationToken ct)
    {
        if (Connected != true)
        {
            log.Error("Not yet connected to Speechmatics, cannot close a current connection");
            return false;
        }

        connectionLifetimeToken.Cancel();
        bool noErrors = true;
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
                try
                {
                    await this.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, closeReason, ct);
                }
                catch (Exception e)
                {
                    log.Error($"Failed to disconnect from Speechmatics: {e.Message}");
                    log.Debug(e.ToString());
                    log.Warning("Assuming that Speechmatics connection is gone now");
                    noErrors = false;
                }

                break;

            default:
                log.Warning("Speechmatics connection was closed by an unknown reason - "
                    + "this may be indicative of an error, unless we're currently testing a new API key");
                break;
        }

        wsClient = null;

        return noErrors;
    }
}
