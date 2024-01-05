namespace Backend.Services;

using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage;
using Backend.Data.SpeechmaticsMessages.AudioAddedMessage;
using Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;
using Backend.Data.SpeechmaticsMessages.EndOfTranscriptMessage;
using Backend.Data.SpeechmaticsMessages.ErrorMessage;
using Backend.Data.SpeechmaticsMessages.InfoMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage;

using Serilog;

public partial class SpeechmaticsSendService : ISpeechmaticsSendService
{
    private ISpeechmaticsConnectionService speechmaticsConnectionService;

    private Serilog.ILogger log;

    public ulong SequenceNumber
    {
        get;
        private set;
    }

    public SpeechmaticsSendService(ISpeechmaticsConnectionService speechmaticsConnectionService, Serilog.ILogger log)
    {
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.log = log;

        resetSequenceNumber();
    }

    private void resetSequenceNumber()
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
