using Backend.Data;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.IO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;



namespace Backend.Services;

/// <summary>
/// Service that monitors the time of the oldest SpeechBubble in the list.
/// </summary>
public class BufferTimeMonitor : BackgroundService
{

    private readonly IHubContext<CommunicationHub> hubContext;

    /// <summary>
    /// List containing all SpeechBubbles that have timed out.
    /// </summary>
    private readonly List<SpeechBubble> timedOutSpeechBubbles;

    private readonly ISpeechBubbleListService speechBubbleListService;

    private readonly ISubtitleExporterService subtitleExporterService;

    private readonly IConfiguration configuration;

    private readonly int timeLimitInMinutes;

    private readonly int delayMilliseconds;

    /// <summary>
    /// Initializes the Dependency Injection and the List of timed out SpeechBubbles.
    /// </summary>
    /// <param name="speechBubbleListService">Service given by the DI</param>
    public BufferTimeMonitor(IConfiguration configuration, IHubContext<CommunicationHub> hubContext,
        ISpeechBubbleListService speechBubbleListService, ISubtitleExporterService subtitleExporterService)
    {
        this.configuration = configuration;
        this.timeLimitInMinutes = configuration.GetValue<int>("BufferTimeMonitorSettings:TimeLimitInMinutes");
        this.delayMilliseconds = configuration.GetValue<int>("BufferTimeMonitorSettings:DelayMilliseconds");
        this.speechBubbleListService = speechBubbleListService;
        this.hubContext = hubContext;
        this.timedOutSpeechBubbles = new List<SpeechBubble>();
        this.subtitleExporterService = subtitleExporterService;
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

            await Task.Delay(delayMilliseconds, stoppingToken);

            var oldestSpeechBubble = speechBubbleListService.GetSpeechBubbles().First;
            if (oldestSpeechBubble == null)
            {
                continue;
            }

            var currentTime = DateTime.Now;
            var oldestSpeechBubbleCreationTime = oldestSpeechBubble.Value.CreationTime;
            var timeDifference = currentTime - oldestSpeechBubbleCreationTime;

            if (timeDifference.TotalMinutes > timeLimitInMinutes)
            {
                await DeleteSpeechBubbleMessageToFrontend(oldestSpeechBubble.Value.Id);

                timedOutSpeechBubbles.Add(oldestSpeechBubble.Value);
                speechBubbleListService.DeleteOldestSpeechBubble();

                await subtitleExporterService.ExportSubtitle(oldestSpeechBubble.Value);
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
            await hubContext.Clients.All.SendAsync("deleteBubble", id);
        }
        catch (Exception)
        {
            await Console.Error.WriteAsync("Failed to transmit id of deleted Speechbubble to Frontend.");
        }
    }
}
