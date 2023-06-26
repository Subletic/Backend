﻿using Backend.Data;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

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

    /// <summary>
    /// Initializes the Dependency Injection and the List of timed out SpeechBubbles.
    /// </summary>
    /// <param name="speechBubbleListService">Service given by the DI</param>
    public BufferTimeMonitor(IHubContext<CommunicationHub> hubContext,
        ISpeechBubbleListService speechBubbleListService)
    {
        _speechBubbleListService = speechBubbleListService;
        _hubContext = hubContext;
        _timedOutSpeechBubbles = new List<SpeechBubble>();
        _timeLimitInMinutes = 5; // move to a constant or configuration file
    }

    /// <summary>
    /// Task that checks every second if the oldest SpeechBubble in the list has overstayed it's welcome.
    /// If the time limit is exceeded, the SpeechBubble is removed from the list and added to the timedOutSpeechBubbles list.
    /// The time limit can be set using the timeLimitInMinutes constant.
    /// </summary>
    /// <param name="stoppingToken">Token used to stop the Task</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var currentTime = DateTime.Now;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);

            var oldestSpeechBubble = _speechBubbleListService.GetSpeechBubbles().First;
            if (oldestSpeechBubble == null)
            {
                continue;
            }
            var oldestSpeechBubbleCreationTime = oldestSpeechBubble.Value.CreationTime;
            var timeDifference = currentTime - oldestSpeechBubbleCreationTime;
            
            if (timeDifference.TotalMinutes > _timeLimitInMinutes)
            {
                _timedOutSpeechBubbles.Add(oldestSpeechBubble.Value);
                _speechBubbleListService.DeleteOldestSpeechBubble();
                
                await DeleteSpeechBubbleMessageToFrontend(oldestSpeechBubble.Value.Id);
            }
 
        }
    }

    /// <summary>
    /// Sends an asynchronous request to the frontend via SignalR, to inform the frontend that a Speechbubble, identified by id, has to be deleted. 
    /// The frontend can then subscribe to incoming Objects and handle them accordingly.
    /// </summary>
    /// <param name="speechBubble"></param>
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