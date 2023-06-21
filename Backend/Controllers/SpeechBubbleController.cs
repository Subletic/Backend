using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    /// <summary>
    /// The SpeechBubbleController handles all incoming requests from the frontend.
    /// </summary>
    [ApiController]
    [Route("api/speechbubble/")]
    public class SpeechBubbleController : ControllerBase
    {
        /// <summary>
        /// Dependency Injection for accessing the LinkedList of SpeechBubbles and corresponding methods.
        /// All actions on the SpeechBubbleList are delegated to the SpeechBubbleListService.
        /// </summary>
        private readonly ISpeechBubbleListService _speechBubbleListService;


        /// <summary>
        /// Constructor for SpeechBubbleController.
        /// Gets instance of SpeechBubbleListService via Dependency Injection.
        /// </summary>
        public SpeechBubbleController(ISpeechBubbleListService speechBubbleListService)
        {
            _speechBubbleListService = speechBubbleListService;
        }


        /// <summary>
        /// The HandleUpdatedSpeechBubble function updates an existing speech bubble with new data.
        /// It accepts a list of speech bubbles.
        /// </summary>
        /// <returns>HTTP Status Code</returns>
        [HttpPost]
        [Route("update")]
        public IActionResult HandleUpdatedSpeechBubble([FromBody] SpeechBubbleChainJson receivedList)
        {
            if (receivedList.SpeechbubbleChain == null) return BadRequest(); // Return the updated _speechBubbleList

            var receivedSpeechBubbles = ParseFrontendResponseToSpeechBubbleList(receivedList);

            // Replace all received SpeechBubbles
            foreach (var receivedSpeechBubble in receivedSpeechBubbles)
            {
                _speechBubbleListService.ReplaceSpeechBubble(receivedSpeechBubble);
            }

            return Ok(); // Return the updated _speechBubbleList
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
                        currentWordToken.Speaker
                    ));
                }

                receivedSpeechBubbles.Add(new SpeechBubble(
                    currentSpeechBubble.Id,
                    currentSpeechBubble.Speaker,
                    currentSpeechBubble.StartTime,
                    currentSpeechBubble.EndTime,
                    receivedWordTokens
                ));
            }

            return receivedSpeechBubbles;
        }
    }
}