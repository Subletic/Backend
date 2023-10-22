namespace Backend.Data.SpeechmaticsMessages.ErrorMessage;

/**
  *  <summary>
  *  Speechmatics RT API message: Error
  *
  *  Direction: Server -> Client
  *  When: Various, depends on <c>type</c>
  *  Purpose: Inform the Client about a critical error that has happened
  *  Effects: Transcription and connection will immediately be terminated
  *
  *  <see cref="type" />
  *  </summary>
  */
public class ErrorMessage
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="code">A numerical code for the error message.</param>
      *  <param name="type">A code for the error message.</param>
      *  <param name="reason">A human-readable reason for the error message.</param>
      *
      *  <see cref="code" />
      *  <see cref="type" />
      *  <see cref="reason" />
      *  </summary>
      */
    public ErrorMessage (int? code, string type, string reason)
    {
        this.code = code;
        this.type = type;
        this.reason = reason;
    }

    /**
      *  <summary>
      *  The internal name of this message.
      *  </summary>
      */
    private static readonly string _message = "Error";

    /**
      *  <value>
      *  Message name.
      *  Settable for JSON deserialising purposes, but value
      *  *MUST* match <c>_message</c> when attempting to set.
      *  </value>
      */
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

    /**
      *  <value>
      *  An optional numerical code for the error message.
      *  </value>
      */
    public int? code { get; set; }

    /**
      *  <value>
      *  A string code for the error message.
      *
      *  Please refer to Speechmatics RT API documentation for the error types.
      *  <see href="https://docs.speechmatics.com/rt-api-ref#error-types" />
      *  </value>
      */
    public string type { get; set; }

    /**
      *  <value>
      *  A human-readable reason for the error message.
      *  </value>
      */
    public string reason { get; set; }
}
