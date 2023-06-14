namespace Backend.Data.SpeechmaticsMessages;

public class RecognitionStartedMessage
{
    public RecognitionStartedMessage (string id, RecognitionStartedMessage_LanguagePackInfo language_pack_info)
    {
        this.id = id;
        this.language_pack_info = language_pack_info;
    }

    private static readonly string _message = "RecognitionStarted";

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
    public string id { get; set; }
    public RecognitionStartedMessage_LanguagePackInfo language_pack_info { get; set; }
}

public class RecognitionStartedMessage_LanguagePackInfo
{
    public RecognitionStartedMessage_LanguagePackInfo (bool adapted, bool itn, string language_description,
        string word_delimiter, string writing_direction)
    {
        this.adapted = adapted;
        this.itn = itn;
        this.language_description = language_description;
        this.word_delimiter = word_delimiter;
        this.writing_direction = writing_direction;
    }

    public bool adapted { get; set; }
    public bool itn { get; set; }
    public string language_description { get; set; }
    public string word_delimiter { get; set; }
    public string writing_direction { get; set; }
}
