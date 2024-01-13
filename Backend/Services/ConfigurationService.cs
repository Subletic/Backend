namespace Backend.Services;

using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using ILogger = Serilog.ILogger;

/// <summary>
/// Dienst zur Verwaltung benutzerdefinierter Wörterbücher.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// List to store custom dictionaries.
    /// </summary>
    private List<StartRecognitionMessage_TranscriptionConfig> customDictionaries;

    /// <summary>
    /// The delay, that is used for time-based waiting in the ConfigurationServiceController and BufferTimeMonitor.
    /// </summary>
    private float delay;

    /// <summary>
    /// Constructor for the ConfigurationService. Initialises the list of custom dictionaries.
    /// </summary>
    /// <param name="logger">Der Logger.</param>
    public ConfigurationService(ILogger logger)
    {
        customDictionaries = new List<StartRecognitionMessage_TranscriptionConfig>();
        this.logger = logger;
    }

    /// <summary>
    /// Process and stores a custom dictionary.
    /// </summary>
    /// <param name="customDictionary">Das zu verarbeitende benutzerdefinierte Wörterbuch.</param>
    /// <exception cref="ArgumentException">Ausgelöst, wenn die Daten des benutzerdefinierten Wörterbuchs ungültig sind.</exception>
    public void ProcessCustomDictionary(StartRecognitionMessage_TranscriptionConfig customDictionary)
    {
        if (customDictionary == null)
        {
            throw new ArgumentException("Invalid custom dictionary data.");
        }

        // Check if the additionalVocab list exceeds the limit.
        if (customDictionary.additional_vocab.Count > 1000)
        {
            throw new ArgumentException("additionalVocab list cannot exceed 1000 elements.");
        }

        // Log information about the received custom dictionary.
        logger.Information($"Received custom dictionary for language {customDictionary.language}");

        // Find an existing dictionary with similar content.
        var existingDictionary = customDictionaries.FirstOrDefault(d =>
            d.additional_vocab.Any(av => av.content == customDictionary.additional_vocab.FirstOrDefault()?.content));

        // If an existing dictionary is found, update it; otherwise, add the new dictionary.
        if (existingDictionary == null)
        {
            customDictionaries.Add(customDictionary);
            logger.Information($"Custom dictionary added to the in-memory data structure for content {customDictionary.additional_vocab.FirstOrDefault()?.content}");
            return;
        }

        existingDictionary = customDictionary;
        foreach (var av in existingDictionary.additional_vocab)
        {
            av.sounds_like = customDictionary.additional_vocab[0].sounds_like;
        }

        logger.Information($"Custom dictionary updated for content {customDictionary.additional_vocab.FirstOrDefault()?.content}");
    }

    /// <summary>
    /// Gets the list of custom dictionaries.
    /// </summary>
    /// <returns>Die Liste der benutzerdefinierten Wörterbücher.</returns>
    public List<StartRecognitionMessage_TranscriptionConfig> GetCustomDictionaries()
    {
        return customDictionaries;
    }

    /// <summary>
    /// Gets the value of the delay.
    /// </summary>
    /// <returns>Die Verzögerung.</returns>
    public float GetDelay()
    {
        return delay;
    }

    /// <summary>
    /// Sets the value of the delay.
    /// </summary>
    /// <param name="delay">Die neue Verzögerung.</param>
    public void SetDelay(float delay)
    {
        this.delay = delay;
    }
}
