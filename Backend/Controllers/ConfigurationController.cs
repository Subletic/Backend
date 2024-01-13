namespace Backend.Controllers;

using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

/// <summary>
/// Controller für benutzerdefinierte Wörterbücher.
/// </summary>
[ApiController]
[Route("api/Configuration")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService dictionaryService;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// Array with valid delayLength values.
    /// </summary>
    private readonly double[] validDelayLengths = { 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9, 10 };

    /// <summary>
    /// Constructor for the CustomDictionaryController. Initialises the custom dictionary service.
    /// </summary>
    /// <param name="dictionaryService">Service for custom dictionaries.</param>
    /// <param name="logger">The logger.</param>
    public ConfigurationController(IConfigurationService dictionaryService, ILogger logger)
    {
        this.dictionaryService = dictionaryService ?? throw new ArgumentNullException(nameof(dictionaryService));
        this.logger = logger;
    }

    /// <summary>
    /// API-Endpoint for uploading a custom dictionary.
    /// </summary>
    /// <param name="configuration">The Transcription-Configuration for the custom dictionary.</param>
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
