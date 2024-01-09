namespace Backend.Services;

/// <summary>
/// Interface for a service that processes audio and video streams.
/// </summary>
public interface IAvProcessingService
{
    /// <summary>
    /// Transcribes the audio stream using Speechmatics.
    /// </summary>
    /// <param name="avStream">The audio and/or video stream to transcribe.</param>
    /// <returns>A task that represents the asynchronous transcription operation. The task result contains true if the transcription was successful, false otherwise.</returns>
    Task<bool> PushProcessedAudio(Stream avStream, CancellationTokenSource ctSource);
}
