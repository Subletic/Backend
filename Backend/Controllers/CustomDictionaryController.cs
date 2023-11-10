using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Backend.Data;
using Backend.Services;

[ApiController]
[Route("api/[controller]")]
public class CustomDictionaryController : ControllerBase
{
    private readonly CustomDictionaryService _dictionaryService;

    public CustomDictionaryController(CustomDictionaryService dictionaryService)
    {
        _dictionaryService = dictionaryService;
    }

    [HttpPost("upload")]
    public IActionResult UploadCustomDictionary([FromBody] CustomDictionary customDictionary)
    {
        if (customDictionary.Equals(default(CustomDictionary)))
        {
            return BadRequest("Invalid custom dictionary data.");
        }

        _dictionaryService.ProcessCustomDictionary(customDictionary);

        return Ok("Custom dictionary uploaded successfully.");
    }
}
