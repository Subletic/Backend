namespace Backend.Services;

/// <summary>
/// A service that is run on startup, and does some initialisation.
/// </summary>
public class StartupService : IHostedService
{
    private const string SPEECHMATICS_API_KEY_ENVVAR = "SPEECHMATICS_API_KEY";

    private readonly IAvProcessingService avProcessingService;

    /// <summary>
    /// Represents a service that handles startup operations.
    /// </summary>
    /// <param name="avProcessingService">The AV processing service.</param>
    public StartupService(IAvProcessingService avProcessingService)
    {
        this.avProcessingService = avProcessingService;
    }

    /// <summary>
    /// Configure the transcription service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Successful Task Completion</returns>
    /// <exception cref="InvalidOperationException">Thrown if AvProcessingService is lacking an Speechmatics API key</exception>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Taking Speechmatics API key from environment variable {SPEECHMATICS_API_KEY_ENVVAR}");
        if (!avProcessingService.Init(SPEECHMATICS_API_KEY_ENVVAR))
            throw new InvalidOperationException("Speechmatics API key is not set");

        return Task.CompletedTask;
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
