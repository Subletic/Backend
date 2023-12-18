namespace Backend.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Service that monitors the time of the oldest SpeechBubble in the list.
/// </summary>
public class BufferTimeMonitor : BackgroundService
{
    private readonly IFrontendCommunicationService frontendCommunicationService;

    private readonly List<SpeechBubble> timedOutSpeechBubbles;

    private readonly ISpeechBubbleListService speechBubbleListService;

    private readonly IConfigurationService configurationService;

    private readonly ISubtitleExporterService subtitleExporterService;

    private readonly IHubContext<FrontendCommunicationHub> hubContext;

    private readonly IConfiguration configuration;

    private readonly int delayMilliseconds;

    private float timeLimitInMinutes;

    /// <summary>
    /// Initializes the Dependency Injection and the List of timed out SpeechBubbles.
    /// </summary>
    /// <param name="frontendProviderService">frontend Provider Service</param>
    /// <param name="configurationService">Configuration service</param>
    /// <param name="configuration">Configuration given by the DI</param>
    /// <param name="hubContext">HubContext given by the DI</param>
    /// <param name="speechBubbleListService">Service that provides access to the active SpeechBubbles</param>
    /// <param name="subtitleExporterService">Service that Exports the Subtitles</param>
    public BufferTimeMonitor(
        IFrontendCommunicationService frontendCommunicationService,
        IHubContext<FrontendCommunicationHub> hubContext,
        IConfigurationService configurationService,
        IConfiguration configuration,
        ISpeechBubbleListService speechBubbleListService,
        ISubtitleExporterService subtitleExporterService)
    {
        this.frontendCommunicationService = frontendCommunicationService;
        this.hubContext = hubContext;
        this.configurationService = configurationService;
        this.configuration = configuration;
        this.timeLimitInMinutes = configuration.GetValue<float>("BufferTimeMonitorSettings:DEFAULT_TIME_LIMIT_IN_MINUTES");
        this.delayMilliseconds = configuration.GetValue<int>("BufferTimeMonitorSettings:DEFAULT_DEALY_MILLISECONDS");
        this.speechBubbleListService = speechBubbleListService;
        this.timedOutSpeechBubbles = new List<SpeechBubble>();
        this.subtitleExporterService = subtitleExporterService;
    }

    /// <summary>
    /// Task that checks every second if the oldest SpeechBubble in the list has overstayed it's welcome.
    /// If the time limit is exceeded, the SpeechBubble is removed from the list and added to the timedOutSpeechBubbles list.
    /// The time limit can be set using the timeLimitInMinutes constant.
    /// </summary>
    /// <param name="stoppingToken">Token used to stop the Task</param>
    /// <returns>Task</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            timeLimitInMinutes = configurationService.GetDelay();
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
                await frontendCommunicationService.DeleteSpeechBubble(oldestSpeechBubble.Value.Id);

                timedOutSpeechBubbles.Add(oldestSpeechBubble.Value);
                speechBubbleListService.DeleteOldestSpeechBubble();

                await subtitleExporterService.ExportSubtitle(oldestSpeechBubble.Value);
            }
        }
    }
}
