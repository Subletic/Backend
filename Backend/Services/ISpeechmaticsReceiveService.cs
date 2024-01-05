namespace Backend.Services;

/// <summary>
/// Interface for a service that handles receiving, decyphering and interpreting
/// messages from Speechmatics.
/// </summary>
public interface ISpeechmaticsReceiveService
{
    /// <summary>
    /// A tracker for the <c>seq_no</c> field of the last-received
    /// <see cref="AudioAddedMessage"/> message. Will be compared against
    /// <see cref="ISpeechmaticsSendService.SequenceNumber"/> at
    /// the end of the communication.
    /// </summary>
    ulong SequenceNumber { get; }

    /// <summary>
    /// Start receiving and processing messages in a loop until an
    /// <see cref="EndOfTranscriptMessage"/> has been received, or
    /// an error is encountered.
    /// </summary>
    /// <returns>Whether or not everything went well</returns>
    Task<bool> ReceiveLoop();

    /// <summary>
    /// Perform a quick test of the complex (and scary-looking) Reflection call
    /// that attempts to find a deserialiser for a message type.
    /// </summary>
    void TestDeserialisation();
}
