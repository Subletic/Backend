namespace Backend.Controllers;

using System;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

/// <summary>
/// Controller for managing custom dictionaries. This controller handles API requests
/// related to the configuration and uploading of custom dictionaries for transcription purposes.
/// </summary>
[ApiController]
[Route("api/Configuration")]
public class ConfigurationController : ControllerBase
{
    // Service for managing custom dictionaries
    private readonly IConfigurationService dictionaryService;

<<<<<<< Backend/Controllers/ConfigurationController.cs
    // Logger for logging within this class
    private readonly ILogger log;

    // Array of valid values for delayLength
    private readonly double[] validDelayLengths = { 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9, 10 };

    /// <summary>
    /// Initializes a new instance of the ConfigurationController class.
    /// Sets up the necessary services for managing custom dictionaries and logging.
    /// </summary>
    /// <param name="dictionaryService">Service for managing custom dictionaries.</param>
    /// <param name="log">Logger for logging activities within the class.</param>
    public ConfigurationController(IConfigurationService dictionaryService, Serilog.ILogger log)
=======
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
>>>>>>> Backend/Controllers/ConfigurationController.cs
    {
        this.dictionaryService = dictionaryService ?? throw new ArgumentNullException(nameof(dictionaryService));
        this.log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <summary>
<<<<<<< Backend/Controllers/ConfigurationController.cs
    /// API endpoint for uploading a custom dictionary. Validates and processes the
    /// uploaded dictionary configuration, ensuring compliance with predefined standards.
    /// </summary>
    /// <param name="configuration">The configuration data for the custom dictionary.</param>
    /// <returns>An ActionResult that reflects the status of the request, such as success or failure.</returns>
=======
    /// API-Endpoint for uploading a custom dictionary.
    /// </summary>
    /// <param name="configuration">The Transcription-Configuration for the custom dictionary.</param>
    /// <returns>ActionResult, das den Status der Anforderung widerspiegelt.</returns>
>>>>>>> Backend/Controllers/ConfigurationController.cs
    [HttpPost("upload")]
    public IActionResult UploadCustomDictionary([FromBody] ConfigurationData? configuration)
    {
        try
        {
            // Loggen der Anforderung zum Hochladen eines benutzerdefinierten Wörterbuchs
            log.Information($"Received request for uploading custom dictionary in {nameof(UploadCustomDictionary)}.");

            // Check if the configuration or the dictionary itself is null
            if (configuration == null || configuration.dictionary == null || configuration.dictionary.additional_vocab == null)
            {
                log.Warning($"Invalid custom dictionary data: Dictionary or its content is null in {nameof(UploadCustomDictionary)}.");
                return BadRequest("Invalid custom dictionary data.");
            }

            // Check if the content is empty and sounds_like is filled
            if (configuration!.dictionary.additional_vocab.Any(av => string.IsNullOrEmpty(av.content) && av.sounds_like != null && av.sounds_like.Any()))
            {
                log.Warning($"Received dictionary with empty content and filled sounds_like in {nameof(UploadCustomDictionary)}.");
                return BadRequest("Invalid custom dictionary data: Dictionaries with empty content and filled sounds_like are not allowed.");
            }

            // Check if the language specified is valid
            if (string.IsNullOrEmpty(configuration!.dictionary.language) || configuration!.dictionary.language != "de")
            {
                log.Warning($"Invalid language specified in {nameof(UploadCustomDictionary)}.");
                return BadRequest("Invalid language specified. Please provide 'de'.");
            }

            // Process the custom dictionary
            dictionaryService.ProcessCustomDictionary(configuration!.dictionary);

            // Check if delayLength is valid
            if (!validDelayLengths.Contains(configuration!.delayLength))
            {
                log.Warning($"Invalid delayLength specified in {nameof(UploadCustomDictionary)}.");
                return BadRequest($"Invalid delayLength specified. Valid values are: {string.Join(" ", validDelayLengths)}.");
            }

            // Set the delayLength
            dictionaryService.SetDelay(configuration!.delayLength);

            // Log the successful upload of the custom dictionary
            log.Information($"Custom dictionary uploaded successfully in {nameof(UploadCustomDictionary)}.");
            return Ok("Custom dictionary uploaded successfully.");
        }
        catch (Exception ex)
        {
            // Log the exception
            log.Error(ex, $"An exception occurred in {nameof(UploadCustomDictionary)}.");
            return StatusCode(500, "Internal Server Error");
        }
    }
}
