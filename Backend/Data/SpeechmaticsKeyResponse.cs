namespace Backend.Data;

public class SpeechmaticsKeyResponse
{
    public SpeechmaticsKeyResponse(string? apikey_id, string key_value) {
        this.apikey_id = apikey_id;
        this.key_value = key_value;
    }

    public string? apikey_id  { get; set; }
    public string key_value { get; set; }
}
