using System.Diagnostics.Tracing;
using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
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
        /// avProcessingService is used to restart the transcription when the frontend calls for a restart.
        /// </summary>
        private readonly ISpeechBubbleListService _speechBubbleListService;

        private readonly IAvProcessingService _avProcessingService;


        /// <summary>
        /// Constructor for SpeechBubbleController.
        /// Gets instance of SpeechBubbleListService via Dependency Injection.
        /// </summary>
        public SpeechBubbleController(ISpeechBubbleListService speechBubbleListService,
            IAvProcessingService avProcessingService)
        {
            _speechBubbleListService = speechBubbleListService;
            _avProcessingService = avProcessingService;
        }


        /// <summary>
        /// The HandleUpdatedSpeechBubble function updates an existing speech bubble with new data.
        /// It accepts a list of speech bubbles.
        /// </summary>
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
                _speechBubbleListService.ReplaceSpeechBubble(receivedSpeechBubble);
            }

            return Ok(); // Return the updated _speechBubbleList
        }

        /// <summary>
        /// Endpoint for restarting the transcription.
        /// </summary>
        /// <returns>Ok if Transcription was successfully started</returns>
        [HttpPost]
        [Route("restart")]
        public IActionResult HandleRestartRequest()
        {
            StartTranscription();
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


        /// <summary>
        /// Starts the transcription of the audio file.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when avProcessingService was not initialized</exception>
        private async void StartTranscription()
        {
            if (_avProcessingService is null)
                throw new InvalidOperationException(
                    $"Failed to find a registered {nameof(IAvProcessingService)} service");

            var doShowcase = await _avProcessingService.Init("SPEECHMATICS_API_KEY");

            Console.WriteLine($"{(doShowcase ? "Doing" : "Not doing")} the Speechmatics API showcase");

            // stressed and exhausted, the compiler is forcing my hand:
            // errors on this variable being unset at the later await, even though it will definitely be set when it needs to await it
            // thus initialise to null and cast away nullness during the await
            Task<bool>? audioTranscription;
            
            if (doShowcase)
            {
                audioTranscription = _avProcessingService.TranscribeAudio("./tagesschau_clip.aac");
            }
            else return;

            var transcriptionSuccess = await audioTranscription;
            
            Console.WriteLine($"Speechmatics communication was a {(transcriptionSuccess ? "success" : "failure")}");
        }
    }
}