using Backend.Data;
using Backend.Hubs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Backend.Controllers
{
    /// <summary>
    /// Controller for handling SpeechBubble interactions.
    /// SpeechBubbles are contained in a LinkedList.
    /// Provides the first SpeechBubble when one of the SpeechBubble split conditions is met.
    /// </summary>
    [ApiController]
    [Route("api/speechbubble")]
    public class SpeechBubbleController : ControllerBase, ISpeechBubbleController
    {
        private readonly IHubContext<CommunicationHub> _hubContext;

        /// <summary>
        /// Dependency Injection for accessing the LinkedList of SpeechBubbles and corresponding methods.
        /// All actions on the SpeechBubbleList are delegated to the SpeechBubbleListService.
        /// </summary>
        private readonly ISpeechBubbleListService _speechBubbleListService;

        private readonly List<WordToken> _wordTokenBuffer;

        private long _nextSpeechBubbleId;
        private int? _currentSpeaker;

        /// <summary>
        /// Constructor for SpeechBubbleController.
        /// Initializes with an empty SpeechBubbleList.
        /// Sets needed private attributes to default values.
        /// </summary>
        public SpeechBubbleController(IHubContext<CommunicationHub> hubContext,
            ISpeechBubbleListService speechBubbleListService)
        {
            _speechBubbleListService = speechBubbleListService;
            _wordTokenBuffer = new List<WordToken>();
            _nextSpeechBubbleId = 1;
            _hubContext = hubContext;
        }


        //Test
        public class SpeechBubbleChainRequest
        {
            public List<SpeechBubble>? SpeechbubbleChain { get; set; }

            public SpeechBubbleChainRequest()
            {
                SpeechbubbleChain = new List<SpeechBubble>();
            }
        }

        /// <summary>
        /// The HandleUpdatedSpeechBubble function updates an existing speech bubble with new data and returns the updated list.
        /// </summary>
        /*
        [HttpPost]
        [Route("update")]
        public IActionResult HandleUpdatedSpeechBubble([FromBody] SpeechBubbleChainRequest request)
        {
            Console.WriteLine("Received SpeechBubbleChainRequest:");
            Console.WriteLine(JsonConvert.SerializeObject(request));

            if (request.SpeechbubbleChain != null)
            {
                Console.WriteLine(JsonConvert.SerializeObject("Betreten 1"));
                foreach (var speechBubble in request.SpeechbubbleChain)
                {
                    Console.WriteLine(JsonConvert.SerializeObject("Betreten 2"));
                    _speechBubbleListService.ReplaceSpeechBubble(speechBubble);
                }
            }

            return Ok(); // Return the updated _speechBubbleList
        }
        */
        [HttpPost]
        [Route("update")]
        public IActionResult HandleUpdatedSpeechBubble([FromBody] SpeechBubbleChainRequest request)
        {
            Console.WriteLine("Received SpeechBubbleChainRequest:");
            Console.WriteLine(JsonConvert.SerializeObject(request));

            if (request.SpeechbubbleChain != null)
            {
                Console.WriteLine(JsonConvert.SerializeObject("Betreten 1"));
                foreach (var speechBubbleData in request.SpeechbubbleChain)
                {
                    Console.WriteLine(JsonConvert.SerializeObject("Betreten 2"));
                    var wordTokens = speechBubbleData.SpeechBubbleContent.Select(w => new WordToken(w.Word, w.Confidence, w.StartTime, w.EndTime, w.Speaker)).ToList();
                    var speechBubble = new SpeechBubble(0, speechBubbleData.Speaker, speechBubbleData.StartTime, speechBubbleData.EndTime, wordTokens);
                    _speechBubbleListService.ReplaceSpeechBubble(speechBubble);
                }
            }

            return Ok(); // Return the updated _speechBubbleList
        }


        [HttpPost]
        [Route("send-new-bubble")]
        public async Task<IActionResult> SendNewSpeechBubble()
        {
            //Console.WriteLine("Received SpeechBubbleSendRequest:");

            // Erstellen Sie hier Ihre neue SpeechBubble und rufen Sie die entsprechende Logik auf

            // Beispiel:

            var newWordToken = new WordToken
            {
                Word = "NeuesWort",
                Confidence = 0.8f,
                StartTime = 3,
                EndTime = 4,
                Speaker = 1
            };
            var existingSpeechBubble = new SpeechBubble(
                id: 1,
                speaker: 1,
                startTime: 10.0,
                endTime: 15.0,
                wordTokens: new List<WordToken> { newWordToken }
            );

            // Senden Sie die neue SpeechBubble an das Frontend
            await SendNewSpeechBubbleMessageToFrontend(existingSpeechBubble);

            return Ok();
        }

        
        /// <summary>
        /// Handles new WordToken given by the Speech-Recognition Software or Mock-Server.
        /// WordTokens are added to a local buffer, which is flushed when the conditions for a new SpeechBubble are met.
        /// Conditions for a new SpeechBubble are contained in the IsSpeechBubbleFull() method.
        /// </summary>
        /// <param name="wordToken">The Word Token that should be appended to a SpeechBubble</param>
        public void HandleNewWord(WordToken wordToken)
        {
            SetSpeakerIfSpeakerIsNull(wordToken);

            // Add new word Token to current Speech Bubble
            if (_currentSpeaker != null && _currentSpeaker == wordToken.Speaker)
            {
                if (IsSpeechBubbleFull(wordToken))
                {
                    FlushBufferToNewSpeechBubble();
                }

                _wordTokenBuffer.Add(wordToken);
            }
            // Finish current SpeechBubble if new Speaker is detected
            else if (_currentSpeaker != null && _currentSpeaker != wordToken.Speaker)
            {
                FlushBufferToNewSpeechBubble();

                _currentSpeaker = wordToken.Speaker;
                _wordTokenBuffer.Add(wordToken);
            }
        }

        /// <summary>
        /// Method to check if the current SpeechBubble is full.
        /// Conditions for determining that a SpeechBubble is full are:
        ///     - A new Speaker is detected
        ///     - The buffer contains 20 words
        ///     - Two consecutive words have a time difference of at least 5 seconds.
        ///
        /// These conditions are held in the constants maxWordCount and maxSecondsTimeDifference and are subject to change.
        /// </summary>
        /// <param name="wordToken">The new Word Token received from the Mock-Server or Speech-Recognition Software</param>
        /// <returns>True if new WordToken should be in a new SpeechBubble</returns>
        private bool IsSpeechBubbleFull(WordToken wordToken)
        {
            const int maxWordCount = 20;
            const int maxSecondsTimeDifference = 5;
            var isTimeLimitExceeded = false;

            if (_wordTokenBuffer.Count > 0)
            {
                var lastBufferElementTimeStamp = _wordTokenBuffer.Last().EndTime;
                var newWordTokenTimeStamp = wordToken.StartTime;
                var timeDifference = newWordTokenTimeStamp - lastBufferElementTimeStamp;

                isTimeLimitExceeded = timeDifference > maxSecondsTimeDifference;
            }

            return _wordTokenBuffer.Count >= maxWordCount - 1 || isTimeLimitExceeded;
        }


        /// <summary>
        /// Method to flush the WordToken buffer to a new SpeechBubble.
        /// Empties WordBuffer and appends new SpeechBubble to the End of the LinkedList
        /// Increments the SpeechBubbleId by 1, so each SpeechBubble has a unique Id.
        /// </summary>
        private async void FlushBufferToNewSpeechBubble()
        {
            var nextSpeechBubble = new SpeechBubble(
                id: _nextSpeechBubbleId,
                speaker: (int)_currentSpeaker!,
                startTime: _wordTokenBuffer.First().StartTime,
                endTime: _wordTokenBuffer.Last().EndTime,
                wordTokens: _wordTokenBuffer
            );

            _nextSpeechBubbleId++;
            _wordTokenBuffer.Clear();
            _speechBubbleListService.AddNewSpeechBubble(nextSpeechBubble);
            await SendNewSpeechBubbleMessageToFrontend(nextSpeechBubble);
        }

        /// <summary>
        /// Method to set the current Speaker if the current Speaker is null.
        /// Should only be called for the first SpeechBubble
        /// </summary>
        /// <param name="wordToken">The new Word Token received from the Mock-Server or Speech-Recognition Software</param>
        private void SetSpeakerIfSpeakerIsNull(WordToken wordToken)
        {
            _currentSpeaker ??= wordToken.Speaker;
        }


        /// <summary>
        /// Sends an asynchronous request to the frontend via SignalR.
        /// The frontend can then subscribe to incoming Objects and handle them accordingly.
        /// </summary>
        /// <param name="speechBubble"></param>
        private async Task SendNewSpeechBubbleMessageToFrontend(SpeechBubble speechBubble)
        {
            var listToSend = new List<SpeechBubble>() { speechBubble };
            
            try
            {
                await _hubContext.Clients.All.SendAsync("newBubble", listToSend);
            }
            catch (Exception)
            {
                await Console.Error.WriteAsync("Failed to transmit to Frontend.");
            }
        }
    }
}