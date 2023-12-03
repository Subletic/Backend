namespace Backend.Controllers;

using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The SpeechBubbleController handles all incoming requests from the frontend.
/// </summary>
[ApiController]
[Route("api/")]
public class SpeechBubbleController : ControllerBase
{
    /// <summary>
    /// Dependency Injection for accessing needed Services.
    /// All actions on the SpeechBubbleList are delegated to the SpeechBubbleListService.
    /// ApplicationLifetime is used to stop the application when the frontend calls for a restart.
    /// </summary>
    private readonly ISpeechBubbleListService speechBubbleListService;

    private readonly IHostApplicationLifetime applicationLifetime;

    /// <summary>
    /// Constructor for SpeechBubbleController.
    /// Gets instance of SpeechBubbleListService via Dependency Injection.
    /// </summary>
    /// <param name="speechBubbleListService">The speech bubble list service.</param>
    /// <param name="applicationLifetime">The application lifetime.</param>
    public SpeechBubbleController(ISpeechBubbleListService speechBubbleListService, IHostApplicationLifetime applicationLifetime)
    {
        this.speechBubbleListService = speechBubbleListService;
        this.applicationLifetime = applicationLifetime;
    }

    /// <summary>
    /// The HandleUpdatedSpeechBubble function updates an existing speech bubble with new data.
    /// It accepts a list of speech bubbles.
    /// </summary>
    /// <param name="receivedList">The received list.</param>
    /// <returns>HTTP Status Code</returns>
    [HttpPost]
    [Route("speechbubble/update")]
    public IActionResult HandleUpdatedSpeechBubble([FromBody] SpeechBubbleChainJson receivedList)
    {
        if (receivedList.SpeechbubbleChain == null) return BadRequest(); // Return the updated _speechBubbleList

        var receivedSpeechBubbles = receivedList.ToSpeechBubbleList();

        // Replace all received SpeechBubbles
        foreach (var receivedSpeechBubble in receivedSpeechBubbles)
        {
            speechBubbleListService.ReplaceSpeechBubble(receivedSpeechBubble);
        }

        return Ok(); // Return the updated _speechBubbleList
    }

    /// <summary>
    /// Endpoint for restarting the application.
    /// Application needs to be started manually again after calling this endpoint.
    /// </summary>
    /// <returns>Ok</returns>
    [HttpPost]
    [Route("restart")]
    public IActionResult HandleRestartRequest()
    {
        applicationLifetime.StopApplication();
        return Ok();
    }
}
