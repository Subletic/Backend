namespace Backend.Services;

public interface ISpeechmaticsReceiveService
{
    Task<bool> ReceiveLoop();

    void TestDeserialisation();
}
