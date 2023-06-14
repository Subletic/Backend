namespace Backend.Data.SpeechmaticsMessages;

/**
  *  <summary>
  *  Speechmatics API response: Temporary Key (no official name)
  *
  *  Direction: Server -> Client
  *  When: After a Temporary Key request for On-demand customers has been requested
  *  Purpose: Give a temporary key for the Speechmatics RT API, valid until
  *  - the configured time-to-live for the key has been reached, or
  *  - the customer has reached their monthly limit for the RT API
  *  Effects: n/a
  *  </summary>
  */
public class TemporaryKeyResponse
{
    /**
      *  <summary>
      *  Simple constructor.
      *
      *  <param name="apikey_id">Unknown, always <c>null</c> in my testing.</param>
      *  <param name="key_value">A temporary key to access the Speechmatics RT API with.</param>
      *
      *  <see cref="apikey_id" />
      *  <see cref="key_value" />
      *  </summary>
      */
    public TemporaryKeyResponse(string? apikey_id, string key_value) {
        this.apikey_id = apikey_id;
        this.key_value = key_value;
    }

    /**
      *  <value>
      *  Unknown, always <c>null</c> in my testing.
      *  </value>
      */
    public string? apikey_id  { get; set; }

    /**
      *  <value>
      *  A temporary key to access the Speechmatics RT API with.
      *  </value>
      */
    public string key_value { get; set; }
}
