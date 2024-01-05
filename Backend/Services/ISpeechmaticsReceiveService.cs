namespace Backend.Services;

public interface ISpeechmaticsReceiveService
{
    ulong SequenceNumber { get; }

    Task<bool> ReceiveLoop();

    void TestDeserialisation();
}
