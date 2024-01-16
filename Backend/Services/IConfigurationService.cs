namespace Backend.Services;

using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

/// <summary>
/// Interface for the custom dictionary service.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Processes and adds a custom dictionary to the service for later use.
    /// </summary>
    /// <param name="newCustomDictionary">The custom dictionary to process.</param>
    void ProcessCustomDictionary(StartRecognitionMessage_TranscriptionConfig newCustomDictionary);

    /// <summary>
    /// Gets the custom dictionaries.
    /// </summary>
    /// <returns>The custom dictionaries.</returns>
    StartRecognitionMessage_TranscriptionConfig? GetCustomDictionary();

    /// <summary>
    /// Sets the delay length for a specific operation.
    /// </summary>
    /// <param name="delay">The delay length to be set.</param>
    public void SetDelay(float delay);

    /// <summary>
    /// Retrieves the delay length for a specific operation.
    /// </summary>
    /// <returns>The delay length.</returns>
    public float GetDelay();
}
