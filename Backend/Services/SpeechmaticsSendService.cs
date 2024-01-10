namespace Backend.Services;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ILogger = Serilog.ILogger;

public class SpeechmaticsSendService : ISpeechmaticsSendService
{
    private ISpeechmaticsConnectionService speechmaticsConnectionService;

    private ILogger log;

    public ulong SequenceNumber
    {
        get;
        private set;
    }

    public SpeechmaticsSendService(ISpeechmaticsConnectionService speechmaticsConnectionService, ILogger log)
    {
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.log = log;

        ResetSequenceNumber();
    }

    public void ResetSequenceNumber()
    {
        SequenceNumber = 0;
    }

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
