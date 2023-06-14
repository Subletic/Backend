namespace Backend.Data.SpeechmaticsMessages;

public class InfoMessage
{
    public InfoMessage (int? code, string type, string reason, string? quality)
    {
        this.code = code;
        this.type = type;
        this.reason = reason;
        this.quality = quality;
    }

    private static readonly string _message = "Info";

    // for JSON deserialising never actually let this be written, just verify
    public string message
    {
        get { return _message; }
        set
        {
            if (value != _message)
                throw new ArgumentException (String.Format (
                    "wrong message type: expected {0}, received {1}",
                    _message, value));
        }
    }
    public int? code { get; set; }
    public string type { get; set; }
    public string reason { get; set; }

    // type: recognition_quality
    public string? quality { get; set; }
}
