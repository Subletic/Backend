namespace Backend.Data.SpeechmaticsMessages;

public class AudioAddedMessage
{
    public AudioAddedMessage (ulong seq_no = 0)
    {
        this.seq_no = seq_no;
    }

    private static readonly string _message = "AudioAdded";

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
    public ulong seq_no { get; set; }
}
