namespace Backend.Services;

public interface IAvProcessingService
{
    // apiKeyVar: envvar that contains the api key to send to speechmatics
    public bool Init (string apiKeyVar);

    public Task<bool> TranscribeAudio (Stream avStream);
}
