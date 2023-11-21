namespace Backend.Services;

/// <summary>
/// Interface for a service that processes audio and video streams.
/// </summary>
public interface IAvProcessingService
{
    /// <summary>
    /// Initializes the service with the API key to send to Speechmatics.
    /// </summary>
    /// <param name="apiKeyVar">Contains the api key to send to Speechmatics.</param>
    /// <returns>True if the initialization was successful, false otherwise.</returns>
    public bool Init(string apiKeyVar);

    /// <summary>
    /// Transcribes the audio stream using Speechmatics.
    /// </summary>
    /// <param name="avStream">The audio and/or video stream to transcribe.</param>
    /// <returns>A task that represents the asynchronous transcription operation. The task result contains true if the transcription was successful, false otherwise.</returns>
    public Task<bool> TranscribeAudio(Stream avStream);
}
