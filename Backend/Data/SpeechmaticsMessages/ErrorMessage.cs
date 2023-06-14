namespace Backend.Data.SpeechmaticsMessages;

public class ErrorMessage
{
    public ErrorMessage (int? code, string type, string reason)
    {
        this.code = code;
        this.type = type;
        this.reason = reason;
    }

    private static readonly string _message = "Error";

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
}
