namespace Backend.Services;

/// <summary>
/// A service that is run on startup, and does some initialisation.
/// </summary>
public class StartupService : IHostedService
{
    private const string SPEECHMATICS_API_KEY_ENVVAR = "SPEECHMATICS_API_KEY";

    private readonly ISpeechmaticsConnectionService speechmaticsConnectionService;

    private readonly ISpeechmaticsReceiveService speechmaticsReceiveService;

    private readonly Serilog.ILogger log;

    /// <summary>
    /// Represents a service that handles startup operations.
    /// </summary>
    /// <param name="speechmaticsConnectionService">The service that handles the connection to the Speechmatics API</param>
    /// <param name="speechmaticsReceiveService">The service that handles the reception of data from the Speechmatics API</param>
    /// <param name="log">The logger</param>
    public StartupService(
        ISpeechmaticsConnectionService speechmaticsConnectionService,
        ISpeechmaticsReceiveService speechmaticsReceiveService,
        Serilog.ILogger log)
    {
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.speechmaticsReceiveService = speechmaticsReceiveService;
        this.log = log;
    }

    /// <summary>
    /// Configure the transcription service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Successful Task Completion</returns>
    /// <exception cref="InvalidOperationException">Thrown if AvProcessingService is lacking an Speechmatics API key</exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        log.Information($"Taking Speechmatics API key from environment variable {SPEECHMATICS_API_KEY_ENVVAR}");
        if (!await speechmaticsConnectionService.RegisterApiKey(SPEECHMATICS_API_KEY_ENVVAR))
            throw new InvalidOperationException("Speechmatics API not available");

        log.Information("Check if Reflection can find deserialisers");
        try
        {
            speechmaticsReceiveService.TestDeserialisation();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Cannot find a deserialiser", e);
        }

        log.Information("Ready for communication");
    }

    /// <summary>
    /// Executed when the service is stopped.
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Successful Task Completion</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
