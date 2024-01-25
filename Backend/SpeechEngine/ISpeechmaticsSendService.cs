namespace Backend.SpeechEngine;

/// <summary>
/// Interface for a service that handles sending binary and text
/// messages to Speechmatics.
/// </summary>
public interface ISpeechmaticsSendService
{
    /// <summary>
    /// Gets a tracker of how many <see cref="SendAudio"/> calls have been
    /// performed. Will be compared against
    /// <see cref="ISpeechmaticsReceiveService.SequenceNumber"/> at
    /// the end of the communication.
    /// </summary>
    ulong SequenceNumber { get; }

    /// <summary>
    /// Reset <see cref="SequenceNumber"/> to 0. To be used when a new
    /// Speechmatics communication has been started.
    /// </summary>
    void ResetSequenceNumber();

    /// <summary>
    /// Send an object (should be of a Speechmatics message type) to Speechmatics
    /// in text mode.
    /// </summary>
    /// <param name="message">The object to send as a text message</param>
    /// <typeparam name="T">The type of the object to send</typeparam>
    /// <returns>Whether sending was successful or not</returns>
    Task<bool> SendJsonMessage<T>(T message);

    /// <summary>
    /// Send a buffer to Speechmatics in binary mode.
    /// This corresponds to a <c>SendAudio</c> message.
    /// </summary>
    /// <param name="audioBuffer">The audio to send</param>
    /// <returns>Whether sending was successful or not</returns>
    Task<bool> SendAudio(byte[] audioBuffer);
}
