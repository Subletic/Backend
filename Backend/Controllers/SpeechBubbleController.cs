﻿using Backend.Data;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Backend.Controllers
{

    /// <summary>
    /// Controller for handling SpeechBubble interactions.
    /// SpeechBubbles are contained in a LinkedList.
    /// Provides the first SpeechBubble when one of the SpeechBubble split conditions is met.
    /// </summary>

    
    [ApiController]
    [Route("api/speechbubble")]

    public class SpeechBubbleController : ISpeechBubbleController
    {
        private readonly IHubContext<CommunicationHub> _hubContext;

        private readonly LinkedList<SpeechBubble> _speechBubbleList;
        private readonly List<WordToken> _wordTokenBuffer;

        private long _nextSpeechBubbleId;
        private int? _currentSpeaker;




        [HttpPost]
        public IActionResult HandleUpdatedSpeechBubble([FromBody] SpeechBubble updatedSpeechBubble)
        {
            var existingSpeechBubble = _speechBubbleList.Find(s => s.Id == updatedSpeechBubble.Id);

            if (existingSpeechBubble != null)
            {
                // Update the existing speech bubble with the new data
                existingSpeechBubble.StartTime = updatedSpeechBubble.StartTime;
                existingSpeechBubble.EndTime = updatedSpeechBubble.EndTime;
                existingSpeechBubble.Speaker = updatedSpeechBubble.Speaker;
                existingSpeechBubble.SpeechBubbleContent = updatedSpeechBubble.SpeechBubbleContent;

                // Perform any additional processing or validation if needed

                // Send the updated speech bubble

                return Ok(_speechBubbleList); // Return the updated _speechBubbleList
            }

            return NotFound("Speech bubble not found");
        }




        /// <summary>
        /// Constructor for SpeechBubbleController.
        /// Initializes with an empty SpeechBubbleList.
        /// Sets needed private attributes to default values.
        /// </summary>
        public SpeechBubbleController(IHubContext<CommunicationHub> hubContext)
        {
            _speechBubbleList = new LinkedList<SpeechBubble>();
            _wordTokenBuffer = new List<WordToken>();
            _nextSpeechBubbleId = 1;
            _hubContext = hubContext;
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
                var lastBufferElementTimeStamp = _wordTokenBuffer.Last().StartTime;
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
            _speechBubbleList.AddLast(nextSpeechBubble);
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
        /// Returns a LinkedList containing all SpeechBubbles.
        /// </summary>
        /// <returns>LinkedList of all SpeechBubbles</returns>
        public LinkedList<SpeechBubble> GetSpeechBubbles()
        {
            return _speechBubbleList;
        }


        /// <summary>
        /// Sends an asynchronous request to the frontend via SignalR.
        /// The frontend can then subscribe to incoming Objects and handle them accordingly.
        /// </summary>
        /// <param name="speechBubble"></param>
        private async Task SendNewSpeechBubbleMessageToFrontend(SpeechBubble speechBubble)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("newBubble", speechBubble);
            }
            catch (Exception)
            {
                await Console.Error.WriteAsync("Failed to transmit to Frontend.");
            }
        }
    }
}