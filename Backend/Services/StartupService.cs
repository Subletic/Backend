namespace Backend.Services;

/// <summary>
/// A service that is run on startup, and does some initialisation.
/// </summary>
public class StartupService : IHostedService
{
    private const string SPEECHMATICS_API_KEY_ENVVAR = "SPEECHMATICS_API_KEY";

    private readonly IAvProcessingService avProcessingService;

    private readonly ISpeechmaticsExchangeService speechmaticsExchangeService;

    private readonly Serilog.ILogger log;

    /// <summary>
    /// Represents a service that handles startup operations.
    /// </summary>
    /// <param name="avProcessingService">The AV processing service.</param>
    public StartupService(
        IAvProcessingService avProcessingService,
        ISpeechmaticsExchangeService speechmaticsExchangeService,
        Serilog.ILogger log)
    {
        this.avProcessingService = avProcessingService;
        this.speechmaticsExchangeService = speechmaticsExchangeService;
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
        if (!await speechmaticsExchangeService.RegisterApiKey(SPEECHMATICS_API_KEY_ENVVAR))
            throw new InvalidOperationException("Speechmatics API not available");
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
