namespace Backend.Data;

public class SpeechmaticsEndOfTranscript
{
    public SpeechmaticsEndOfTranscript () { }

    private static readonly string _message = "EndOfTranscript";

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
}
