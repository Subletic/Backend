namespace Backend.Services;


/// <summary>
/// A service that is run on startup, and does some initialisation.
/// </summary>
public class StartupService : IHostedService
{
    private static readonly Uri showcaseUri = new Uri (
        "https://cdn.discordapp.com/attachments/1119718406791376966/1123317245246976010/tagesschau_clip.aac");

    private readonly IAvProcessingService _avProcessingService;

    public StartupService(IAvProcessingService avProcessingService)
    {
        _avProcessingService = avProcessingService;
    }

    /// <summary>
    /// Starts the transcription service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <exception cref="InvalidOperationException">Thrown if no AvProcessingService is running</exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting up the transcription service...");

        // TODO manually kick off a transcription, for testing
        if (_avProcessingService is null)
            throw new InvalidOperationException(
                $"Failed to find a registered {nameof(IAvProcessingService)} service");

        var doShowcase = await _avProcessingService.Init("SPEECHMATICS_API_KEY");

        Console.WriteLine($"{(doShowcase ? "Doing" : "Not doing")} the Speechmatics API showcase");

        // stressed and exhausted, the compiler is forcing my hand:
        // errors on this variable being unset at the later await, even though it will definitely be set when it needs to await it
        // thus initialise to null and cast away nullness during the await


        if (doShowcase)
        {
            var audioTranscription = _avProcessingService.TranscribeAudio(showcaseUri);
            // var transcriptionSuccess = await audioTranscription;
            // Console.WriteLine($"Speechmatics communication was a {(transcriptionSuccess ? "success" : "failure")}");
        }
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
