namespace Backend.Services;

public interface ISpeechmaticsSendService
{
    Task<bool> SendJsonMessage<T>(T message);

    Task<bool> SendAudio(byte[] audioBuffer);
}
