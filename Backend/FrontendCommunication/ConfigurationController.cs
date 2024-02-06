namespace Backend.FrontendCommunication;

using System;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.SpeechEngine;
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

    // Service for managing Speechmatics connection
    private readonly ISpeechmaticsConnectionService speechmaticsConnectionService;

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
    public ConfigurationController(
        IConfigurationService dictionaryService,
        ISpeechmaticsConnectionService speechmaticsConnectionService,
        Serilog.ILogger log)
    {
        this.dictionaryService = dictionaryService;
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.log = log;
    }

    /// <summary>
    /// API endpoint for uploading a custom dictionary. Validates and processes the
    /// uploaded dictionary configuration, ensuring compliance with predefined standards.
    /// </summary>
    /// <param name="configuration">The configuration data for the custom dictionary.</param>
    /// <returns>An ActionResult that reflects the status of the request, such as success or failure.</returns>
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

            // TODO: This could use some better handling to avoid TOCTOU, but
            // the worst case here is an incorrect, non-critical message to the user
            bool speechmaticsAlreadyConnected = speechmaticsConnectionService.Connected;

            // Process the custom dictionary
            dictionaryService.ProcessCustomDictionary(configuration!.dictionary);

            // Log the successful upload of the custom dictionary
            log.Information($"Custom dictionary uploaded successfully in {nameof(UploadCustomDictionary)}.");

            // Check if delayLength is valid
            if (!validDelayLengths.Contains(configuration!.delayLength))
            {
                log.Warning($"Invalid delayLength specified in {nameof(UploadCustomDictionary)}.");
                return BadRequest($"Invalid delayLength specified. Valid values are: {string.Join(" ", validDelayLengths)}.");
            }

            // Set the delayLength
            dictionaryService.SetDelay(configuration!.delayLength);

            if (!speechmaticsAlreadyConnected)
                return Ok("Configuration uploaded successfully.");
            else
                return Accepted("Custom dictionary will be used on the next connection");
        }
        catch (Exception ex)
        {
            // Log the exception
            log.Error(ex, $"An exception occurred in {nameof(UploadCustomDictionary)}.");
            return StatusCode(500, "Internal Server Error");
        }
    }
}
