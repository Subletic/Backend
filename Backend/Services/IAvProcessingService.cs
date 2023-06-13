namespace Backend.Services;

public interface IAvProcessingService
{
    // apiKeyVar: envvar that contains the api key to send to speechmatics
    public Task Init (string apiKeyVar);

    public Task<bool> TranscribeAudio (string filepath);
}
