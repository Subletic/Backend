﻿using Backend.Data;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.IO;



namespace Backend.Services;

/// <summary>
/// Service that monitors the time of the oldest SpeechBubble in the list.
/// </summary>
public class BufferTimeMonitor : BackgroundService
{
    
    private readonly IHubContext<CommunicationHub> _hubContext;
    
    /// <summary>
    /// List containing all SpeechBubbles that have timed out.
    /// </summary>
    private readonly List<SpeechBubble> _timedOutSpeechBubbles;

    private readonly ISpeechBubbleListService _speechBubbleListService;
    
    private readonly int _timeLimitInMinutes;

    private readonly WebVttExporter _webVttExporter;

    private readonly MemoryStream _outputStream;

    private const int DELAY_MILLISECONDS = 1000;

    private const int SPEECH_BUBBLE_VALIDITY_MINUTES = 1;


    /// <summary>
    /// Initializes the Dependency Injection and the List of timed out SpeechBubbles.
    /// </summary>
    /// <param name="speechBubbleListService">Service given by the DI</param>
    public BufferTimeMonitor(IHubContext<CommunicationHub> hubContext, ISpeechBubbleListService speechBubbleListService, WebVttExporter webVttExporter)
    {
        _speechBubbleListService = speechBubbleListService;
        _hubContext = hubContext;
        _timedOutSpeechBubbles = new List<SpeechBubble>();
        _timeLimitInMinutes = SPEECH_BUBBLE_VALIDITY_MINUTES; // move to a constant or configuration file
        _webVttExporter = webVttExporter;
        _outputStream = new MemoryStream();
    }

    /// <summary>
    /// Task that checks every second if the oldest SpeechBubble in the list has overstayed it's welcome.
    /// If the time limit is exceeded, the SpeechBubble is removed from the list and added to the timedOutSpeechBubbles list.
    /// The time limit can be set using the timeLimitInMinutes constant.
    /// </summary>
    /// <param name="stoppingToken">Token used to stop the Task</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            
            await Task.Delay(DELAY_MILLISECONDS, stoppingToken);

            var oldestSpeechBubble = _speechBubbleListService.GetSpeechBubbles().First;
            if (oldestSpeechBubble == null)
            {
                continue;
            }
            
            var currentTime = DateTime.Now;
            var oldestSpeechBubbleCreationTime = oldestSpeechBubble.Value.CreationTime;
            var timeDifference = currentTime - oldestSpeechBubbleCreationTime;
            
            
            if (timeDifference.TotalMinutes > _timeLimitInMinutes)
            {
                
                await DeleteSpeechBubbleMessageToFrontend(oldestSpeechBubble.Value.Id);
                
                _timedOutSpeechBubbles.Add(oldestSpeechBubble.Value);
                _speechBubbleListService.DeleteOldestSpeechBubble();

                // Export timed-out speech bubble as webvtt
                using (var outputStream = new MemoryStream())
                {
                    _webVttExporter.ExportSpeechBubble(oldestSpeechBubble.Value);
                    outputStream.Seek(0, SeekOrigin.Begin);
                    await outputStream.CopyToAsync(_outputStream, stoppingToken);
                }
            }
 
        }
    }

    /// <summary>
    /// Sends an asynchronous request to the frontend via SignalR, to inform the frontend that a Speechbubble, identified by id, has to be deleted. 
    /// The frontend can then subscribe to incoming Objects and handle them accordingly.
    /// </summary>
    /// <param name="id"></param>
    private async Task DeleteSpeechBubbleMessageToFrontend(long id)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("deleteBubble", id);
        }
        catch (Exception)
        {
            await Console.Error.WriteAsync("Failed to transmit id of deleted Speechbubble to Frontend.");
        }
    }
}