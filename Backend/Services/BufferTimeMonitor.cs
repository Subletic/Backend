using Backend.Data;

namespace Backend.Services;

/// <summary>
/// Service that monitors the time of the oldest SpeechBubble in the list.
/// </summary>
public class BufferTimeMonitor : BackgroundService
{
    /// <summary>
    /// List containing all SpeechBubbles that have timed out.
    /// </summary>
    private readonly List<SpeechBubble> _timedOutSpeechBubbles;

    private readonly ISpeechBubbleListService _speechBubbleListService;

    /// <summary>
    /// Initializes the Dependency Injection and the List of timed out SpeechBubbles.
    /// </summary>
    /// <param name="speechBubbleListService">Service given by the DI</param>
    public BufferTimeMonitor(ISpeechBubbleListService speechBubbleListService)
    {
        _speechBubbleListService = speechBubbleListService;
        _timedOutSpeechBubbles = new List<SpeechBubble>();
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
            await Task.Delay(1000, stoppingToken);

            var oldestSpeechBubble = _speechBubbleListService.GetSpeechBubbles().First;
            if (oldestSpeechBubble == null)
            {
                continue;
            }
            
            var oldestSpeechBubbleCreationTime = oldestSpeechBubble.Value.CreationTime;
            var currentTime = DateTime.Now;
            var timeDifference = currentTime - oldestSpeechBubbleCreationTime;
            
            // fix magic number
            const int timeLimitInMinutes = 5;

            if (timeDifference.TotalMinutes > timeLimitInMinutes)
            {
                _timedOutSpeechBubbles.Add(oldestSpeechBubble.Value);
                _speechBubbleListService.DeleteOldestSpeechBubble();


                // Export timed-out speech bubbles as webvtt
                using (var outputStream = new MemoryStream())
                {
                    // TODO: Implement the conversion logic
                }
            }
        }
    }
}