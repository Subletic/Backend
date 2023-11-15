using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

/**
 * <summary>
 * Controller für benutzerdefinierte Wörterbücher.
 * </summary>
 */
namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomDictionaryController : ControllerBase
    {
        private readonly ICustomDictionaryService _dictionaryService;

        /**
         * <summary>
         * Konstruktor für den CustomDictionaryController. Initialisiert den Dienst für benutzerdefinierte Wörterbücher.
         * </summary>
         * <param name="dictionaryService">Der Dienst für benutzerdefinierte Wörterbücher.</param>
         */
        public CustomDictionaryController(ICustomDictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService ?? throw new ArgumentNullException(nameof(dictionaryService));
        }

        /**
         * <summary>
         * API-Endpunkt zum Hochladen eines benutzerdefinierten Wörterbuchs.
         * </summary>
         * <param name="transcriptionConfig">Die Transkriptionskonfiguration für das benutzerdefinierte Wörterbuch.</param>
         * <returns>ActionResult, das den Status der Anforderung widerspiegelt.</returns>
         */
        [HttpPost("upload")]
        public IActionResult UploadCustomDictionary([FromBody] StartRecognitionMessage_TranscriptionConfig transcriptionConfig)
        {
            try
            {
                if (transcriptionConfig == null || transcriptionConfig.additionalVocab == null)
                {
                    return BadRequest("Invalid custom dictionary data.");
                }

                // Erstellen Sie eine Instanz von Dictionary und übergeben Sie sie an den Service
                var customDictionary = new Dictionary(transcriptionConfig);
                _dictionaryService.ProcessCustomDictionary(customDictionary);

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
}
