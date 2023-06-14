namespace Backend.Data.SpeechmaticsMessages;

public class WarningMessage
{
    public WarningMessage (int? code, string type, string reason, ulong? duration_limit)
    {
        this.code = code;
        this.type = type;
        this.reason = reason;
        this.duration_limit = duration_limit;
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

    // type: duration_limit_exceeded
    public ulong? duration_limit { get; set; }
}
