namespace Backend.Controllers;

using System;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

/// <summary>
/// Controller für benutzerdefinierte Wörterbücher.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomDictionaryController : ControllerBase
{
    private readonly ICustomDictionaryService dictionaryService;

    /// <summary>
    /// Konstruktor für den CustomDictionaryController. Initialisiert den Dienst für benutzerdefinierte Wörterbücher.
    /// </summary>
    /// <param name="dictionaryService">Der Dienst für benutzerdefinierte Wörterbücher.</param>
    public CustomDictionaryController(ICustomDictionaryService dictionaryService)
    {
        this.dictionaryService = dictionaryService ?? throw new ArgumentNullException(nameof(dictionaryService));
    }

    /// <summary>
    /// API-Endpunkt zum Hochladen eines benutzerdefinierten Wörterbuchs.
    /// </summary>
    /// <param name="transcriptionConfig">Die Transkriptionskonfiguration für das benutzerdefinierte Wörterbuch.</param>
    /// <returns>ActionResult, das den Status der Anforderung widerspiegelt.</returns>
    [HttpPost("upload")]
    public IActionResult UploadCustomDictionary([FromBody] StartRecognitionMessage_TranscriptionConfig transcriptionConfig)
    {
        try
        {
            if (transcriptionConfig == null || transcriptionConfig.additional_vocab == null)
            {
                return BadRequest("Invalid custom dictionary data.");
            }

            // Erstellen Sie eine Instanz von Dictionary und übergeben Sie sie an den Service
            var customDictionary = new Dictionary(transcriptionConfig);
            dictionaryService.ProcessCustomDictionary(customDictionary);

            return Ok("Custom dictionary uploaded successfully.");
        }
        catch (Exception ex)
        {
            // Loggen Sie die Ausnahme oder führen Sie andere Aktionen durch
            Log.Error(ex, "An exception occurred.");
            return StatusCode(500, "Internal Server Error");
        }
    }
}
