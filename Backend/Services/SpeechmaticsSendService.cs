namespace Backend.Services;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ILogger = Serilog.ILogger;

/// <summary>
/// Service to send messages to Speechmatics.
/// </summary>
public class SpeechmaticsSendService : ISpeechmaticsSendService
{
    private ISpeechmaticsConnectionService speechmaticsConnectionService;

    private ILogger log;

    /// <summary>
    /// Gets the sequence number of the last audio chunk that was sent to Speechmatics.
    /// </summary>
    public ulong SequenceNumber
    {
        get;
        private set;
    }

    /// <summary>
    /// Constructor for the SpeechmaticsSendService.
    /// </summary>
    /// <param name="speechmaticsConnectionService">Service that handles the connection to Speechmatics.</param>
    /// <param name="log">The logger.</param>
    public SpeechmaticsSendService(ISpeechmaticsConnectionService speechmaticsConnectionService, ILogger log)
    {
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.log = log;

        ResetSequenceNumber();
    }

    /// <summary>
    /// Resets the sequence number to 0.
    /// </summary>
    public void ResetSequenceNumber()
    {
        SequenceNumber = 0;
    }

    /// <summary>
    /// Sends a JSON message to Speechmatics.
    /// </summary>
    /// <typeparam name="T">Type of the message to send.</typeparam>
    /// <param name="message">The message to send.</param>
    /// <returns>True if the message was sent successfully, false otherwise.</returns>
    public async Task<bool> SendJsonMessage<T>(T message)
    {
        speechmaticsConnectionService.ThrowIfNotConnected();

        byte[] messageSerialised = JsonSerializer.SerializeToUtf8Bytes<T>(message, speechmaticsConnectionService.JsonOptions);
        log.Information($"Sending {typeof(T).Name} message to Speechmatics");
        log.Debug(Encoding.UTF8.GetString(messageSerialised));

        await speechmaticsConnectionService.Socket.SendAsync(
            buffer: messageSerialised,
            messageType: WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: speechmaticsConnectionService.CancellationToken);

        // FIXME return some failure/success indicator
        return true;
    }

    /// <summary>
    /// Sends an audio chunk to Speechmatics.
    /// </summary>
    /// <param name="audioBuffer">The audio chunk to send.</param>
    /// <returns>True if the audio chunk was sent successfully, false otherwise.</returns>
    public async Task<bool> SendAudio(byte[] audioBuffer)
    {
        speechmaticsConnectionService.ThrowIfNotConnected();

        log.Information("Sending SendAudio message to Speechmatics");

        await speechmaticsConnectionService.Socket.SendAsync(
            buffer: audioBuffer,
            messageType: WebSocketMessageType.Binary,
            endOfMessage: true,
            cancellationToken: speechmaticsConnectionService.CancellationToken);

        SequenceNumber += 1;

        // FIXME return some failure/success indicator
        return true;
    }
}
