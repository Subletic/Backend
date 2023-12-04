namespace Backend.Services;

public interface ISpeechmaticsExchangeService
{
    /// <summary>
    /// Registers the API key to use with the Speechmatics RT API.
    /// </summary>
    /// <param name="apiKeyVar">Contains the api key to send to Speechmatics.</param>
    /// <returns>True if the envvar was set and Speechmatics accepts its value, false otherwise.</returns>
    public Task<bool> RegisterApiKey(string apiKeyVar);

    public Task<bool> Connect(CancellationTokenSource cts);

    public Task<bool> Disconnect(CancellationTokenSource cts);
}
