namespace Backend.Data;

public struct SpeechmaticsKeyResponse
{
    public SpeechmaticsKeyResponse(string? apikeyId, string keyValue) {
        apikey_id = apikeyId;
        key_value = keyValue;
    }

    public string? apikey_id  { get; set; }
    public string key_value { get; set; }
}
