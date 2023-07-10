namespace Backend.Services;

public interface IAvProcessingService
{
    // apiKeyVar: envvar that contains the api key to send to speechmatics
    public Task<bool> Init (string apiKeyVar);

    public Task<bool> TranscribeAudio (Uri mediaUri);
}
