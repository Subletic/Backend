namespace Backend.Data.SpeechmaticsMessages;

public class EndOfStreamMessage
{
    public EndOfStreamMessage (ulong last_seq_no = 0)
    {
        this.last_seq_no = last_seq_no;
    }

    private static readonly string _message = "EndOfStream";

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
    public ulong last_seq_no { get; set; }
}
