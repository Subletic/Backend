using Backend.Data; 
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using Serilog;
using Serilog.Events;


[ApiController]
[Route("api/[controller]")]
public class CustomDictionaryController : ControllerBase
{
    private readonly CustomDictionaryService _dictionaryService;

    public CustomDictionaryController(CustomDictionaryService dictionaryService)
    {
        _dictionaryService = dictionaryService;
    }

    [HttpPost("upload-custom-dictionary")]
    public IActionResult UploadCustomDictionary([FromBody] TranscriptionConfig transcriptionConfig)
    {
        try
        {
            if (transcriptionConfig == null || transcriptionConfig.AdditionalVocab == null)
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
            Log.Error(ex, "Eine Ausnahme ist aufgetreten.");
            return StatusCode(500, "Internal Server Error");
        }

    }
}

