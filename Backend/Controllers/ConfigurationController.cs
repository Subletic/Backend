namespace Backend.Controllers;

using System;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller für benutzerdefinierte Wörterbücher.
/// </summary>
[ApiController]
[Route("api/Configuration")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService dictionaryService;

    /// <summary>
    /// Konstruktor für den CustomDictionaryController. Initialisiert den Dienst für benutzerdefinierte Wörterbücher.
    /// </summary>
    /// <param name="dictionaryService">Der Dienst für benutzerdefinierte Wörterbücher.</param>
    public ConfigurationController(IConfigurationService dictionaryService)
    {
        this.dictionaryService = dictionaryService ?? throw new ArgumentNullException(nameof(dictionaryService));
    }

    /// <summary>
    /// API-Endpunkt zum Hochladen eines benutzerdefinierten Wörterbuchs.
    /// </summary>
    /// <param name="transcriptionConfig">Die Transkriptionskonfiguration für das benutzerdefinierte Wörterbuch.</param>
    /// <returns>ActionResult, das den Status der Anforderung widerspiegelt.</returns>
    [HttpPost("upload")]
    public IActionResult UploadCustomDictionary([FromBody] ConfigurationData configuration)
    {
        try
        {
            if (configuration == null)
            {
                return BadRequest("Invalid custom dictionary data.");
            }

            // Überprüfen, ob die Konfiguration gültig ist und keine leere Instanz ist.
            // Wenn das benutzerdefinierte Wörterbuch in der Konfiguration vorhanden ist, verarbeiten und übergeben Sie es an den Service.
            if (configuration.dictionary.transcription_config.additional_vocab != null)
            {
                // Validate empty content with filled sounds_like in additional_vocab
                if (configuration.dictionary.transcription_config.additional_vocab.Any(av => string.IsNullOrEmpty(av.content) && av.sounds_like != null && av.sounds_like.Any()))
                {
                    return BadRequest("Invalid custom dictionary data: Dictionaries with empty content and filled sounds_like are not allowed.");
                }

                // Validate language
                if (string.IsNullOrEmpty(configuration.dictionary.transcription_config.language) ||
                    (configuration.dictionary.transcription_config.language != "de" && configuration.dictionary.transcription_config.language != "en"))
                {
                    return BadRequest("Invalid language specified. Please provide 'de' or 'en'.");
                }

                var customDictionary = new Dictionary(configuration.dictionary.transcription_config);
                dictionaryService.ProcessCustomDictionary(customDictionary);
            }

            // Die Verzögerungslänge aus der Konfiguration festlegen.
            if (configuration.delayLength > 0.5 && configuration.delayLength <= 10.0)
            {
                dictionaryService.SetDelay(configuration.delayLength);
            }
            
            return Ok("Custom dictionary uploaded successfully.");
        }
        catch (Exception ex)
        {
            // Loggen Sie die Ausnahme oder führen Sie andere Aktionen durch
            Console.WriteLine($"An exception occurred: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }
}
