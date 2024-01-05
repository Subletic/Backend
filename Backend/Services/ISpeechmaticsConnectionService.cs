namespace Backend.Services;

using System.Net.WebSockets;
using System.Text.Json;

using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;

public interface ISpeechmaticsConnectionService
{
    StartRecognitionMessage_AudioFormat AudioFormat { get; }

    ClientWebSocket Socket { get; }

    JsonSerializerOptions JsonOptions { get; }

    CancellationToken CancellationToken { get; }

    bool Connected { get; }

    void ThrowIfNotConnected();

    /// <summary>
    /// Registers the API key to use with the Speechmatics RT API.
    /// </summary>
    /// <param name="apiKeyVar">Contains the api key to send to Speechmatics.</param>
    /// <returns>True if the envvar was set and Speechmatics accepts its value, false otherwise.</returns>
    Task<bool> RegisterApiKey(string apiKeyVar);

    Task<bool> Connect(CancellationToken ct);

    Task<bool> Disconnect(bool signalSuccess, CancellationToken ct);
}
