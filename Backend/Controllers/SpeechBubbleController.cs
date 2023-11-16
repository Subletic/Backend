namespace Backend.Controllers
{
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

            var receivedSpeechBubbles = ParseFrontendResponseToSpeechBubbleList(receivedList);

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

        /// <summary>
        /// Parses incoming JSON from the frontend to a list of backend-compatible SpeechBubbles.
        /// </summary>
        /// <param name="receivedList">The non-empty json received</param>
        /// <returns>List of parsed SpeechBubbles</returns>
        public static List<SpeechBubble> ParseFrontendResponseToSpeechBubbleList(SpeechBubbleChainJson receivedList)
        {
            var receivedSpeechBubbles = new List<SpeechBubble>();

            foreach (var currentSpeechBubble in receivedList.SpeechbubbleChain!)
            {
                var receivedWordTokens = new List<WordToken>();
                foreach (var currentWordToken in currentSpeechBubble.SpeechBubbleContent)
                {
                    receivedWordTokens.Add(new WordToken(
                        currentWordToken.Word,
                        currentWordToken.Confidence,
                        currentWordToken.StartTime,
                        currentWordToken.EndTime,
                        currentWordToken.Speaker));
                }

                receivedSpeechBubbles.Add(new SpeechBubble(
                    currentSpeechBubble.Id,
                    currentSpeechBubble.Speaker,
                    currentSpeechBubble.StartTime,
                    currentSpeechBubble.EndTime,
                    receivedWordTokens));
            }

            return receivedSpeechBubbles;
        }
    }
}
