namespace Backend.Services;

public interface ISpeechmaticsSendService
{
    ulong SequenceNumber { get; }

    Task<bool> SendJsonMessage<T>(T message);

    Task<bool> SendAudio(byte[] audioBuffer);
}
