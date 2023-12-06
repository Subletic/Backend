namespace Backend.Controllers;

using System;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;

/// <summary>
/// Controller für benutzerdefinierte Wörterbücher.
/// </summary>
[ApiController]
[Route("api/Configuration")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService dictionaryService;

    // Das private readonly Feld logger wird verwendet, um den Logger für die Protokollierung innerhalb dieser Klasse zu halten.
    private readonly Serilog.ILogger logger;

    // Array mit gültigen delayLength-Werten
    private readonly double[] validDelayLengths = { 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9, 10 };

    /// <summary>
    /// Konstruktor für den CustomDictionaryController. Initialisiert den Dienst für benutzerdefinierte Wörterbücher.
    /// </summary>
    /// <param name="dictionaryService">Der Dienst für benutzerdefinierte Wörterbücher.</param>
    public ConfigurationController(IConfigurationService dictionaryService, Serilog.ILogger logger)
    {
        this.dictionaryService = dictionaryService ?? throw new ArgumentNullException(nameof(dictionaryService));
        this.logger = logger;
    }

    /// <summary>
    /// API-Endpunkt zum Hochladen eines benutzerdefinierten Wörterbuchs.
    /// </summary>
    /// <param name="configuration">Die Transkriptionskonfiguration für das benutzerdefinierte Wörterbuch.</param>
    /// <returns>ActionResult, das den Status der Anforderung widerspiegelt.</returns>
    [HttpPost("upload")]
    public IActionResult UploadCustomDictionary([FromBody] ConfigurationData? configuration)
    {
        try
        {
            logger.Information("Received request for uploading custom dictionary.");

            if (configuration == null || configuration.dictionary == null || configuration.dictionary.additional_vocab == null)
            {
                logger.Warning("Invalid custom dictionary data: Dictionary or its content is null.");
                return BadRequest("Invalid custom dictionary data.");
            }

            // Überprüfen, ob die Konfiguration gültig ist und keine leere Instanz ist.
            // Validate empty content with filled sounds_like in additional_vocab
            if (configuration!.dictionary.additional_vocab.Any(av => string.IsNullOrEmpty(av.content) && av.sounds_like != null && av.sounds_like.Any()))
            {
                logger.Warning("Received dictionary with empty content and filled sounds_like.");
                return BadRequest("Invalid custom dictionary data: Dictionaries with empty content and filled sounds_like are not allowed.");
            }

            // Validate language
            if (string.IsNullOrEmpty(configuration!.dictionary.language) || configuration!.dictionary.language != "de")
            {
                logger.Warning("Invalid language specified.");
                return BadRequest("Invalid language specified. Please provide 'de'.");
            }

            dictionaryService.ProcessCustomDictionary(configuration!.dictionary);

            // Validate delayLength
            if (!validDelayLengths.Contains(configuration!.delayLength))
            {
                logger.Warning("Invalid delayLength specified.");
                return BadRequest($"Invalid delayLength specified. Valid values are: {string.Join(" ", validDelayLengths)}.");
            }

            dictionaryService.SetDelay(configuration!.delayLength);

            logger.Information("Custom dictionary uploaded successfully.");
            return Ok("Custom dictionary uploaded successfully.");
        }
        catch (Exception ex)
        {
            // Loggen Sie die Ausnahme oder führen Sie andere Aktionen durch
            logger.Error(ex, "An exception occurred.");
            return StatusCode(500, "Internal Server Error");
        }
    }
}
