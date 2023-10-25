namespace Backend.Data.SpeechmaticsMessages.WarningMessage;

/**
  *  <summary>
  *  Speechmatics RT API message: Warning
  *
  *  Direction: Server -> Client
  *  When: Various, depends on <c>type</c>
  *  Purpose: Warn the Client about something that may become a problem
  *  Effects: Various, depends on <c>type</c>
  *  API Reference: <see href="https://docs.speechmatics.com/rt-api-ref#warning" />
  *
  *  <see cref="type" />
  *  </summary>
  */
public class WarningMessage
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="code">A numerical code for the warning message.</param>
      *  <param name="type">A code for the warning message.</param>
      *  <param name="reason">A human-readable reason for the warning message.</param>
      *  <param name="duration_limit">
      *  In case of <c>duration_limit_exceeded</c>, the limit that was exceeded (in seconds".
      *  </param>
      *
      *  <see cref="code" />
      *  <see cref="type" />
      *  <see cref="reason" />
      *  <see cref="duration_limit" />
      *  </summary>
      */
    public WarningMessage (int? code, string type, string reason, ulong? duration_limit)
    {
        this.code = code;
        this.type = type;
        this.reason = reason;
        this.duration_limit = duration_limit;
    }

    /**
      *  <summary>
      *  The internal name of this message.
      *  </summary>
      */
    private const string MESSAGE_TYPE = "Info";

    /**
      *  <value>
      *  Message name.
      *  Settable for JSON deserialising purposes, but value
      *  *MUST* match <c>MESSAGE_TYPE</c> when attempting to set.
      *  </value>
      */
    public string message
    {
        get { return MESSAGE_TYPE; }
        set
        {
            if (value != MESSAGE_TYPE)
                throw new ArgumentException (String.Format (
                    "wrong message type: expected {0}, received {1}",
                    MESSAGE_TYPE, value));
        }
    }

    /**
      *  <value>
      *  An optional numerical code for the warning message.
      *  </value>
      */
    public int? code { get; set; }

    /**
      *  <value>
      *  A string code for the warning message.
      *
      *  <c>duration_limit_exceeded</c>:
      *  - When: The maximum allowed duration of a single utterance to process has been exceeded.
      *  - Effects:
      *    - further <c>AddAudio</c> messages will be acknowledged with a <c>AudioAddedMessage</c>, but ignored
      *    - Server will act as if <c>EndOfStream</c> was sent
      *
      *  <c>unsupported_translation_pair</c>:
      *  - When: One of the requested translation target languages is unsupported.
      *  - Effects: ?
      *  </value>
      */
    public string type { get; set; }

    /**
      *  <value>
      *  A human-readable reason for the warning message.
      *  </value>
      */
    public string reason { get; set; }

    /**
      *  <value>
      *  If <c>duration_limit_exceeded</c>, the limit that was exceeded (in seconds).
      *  </value>
      */
    public ulong? duration_limit { get; set; }
}
