namespace Backend.SpeechEngine;

using System.Net.WebSockets;
using System.Text.Json;

using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;

/// <summary>
/// Interface for a service that handles a WebSocket connection to Speechmatics
/// and shared details about the communication.
/// </summary>
public interface ISpeechmaticsConnectionService
{
    /// <summary>
    /// Gets what audio format will be sent over the connection.
    /// </summary>
    StartRecognitionMessage_AudioFormat AudioFormat { get; }

    /// <summary>
    /// Gets a <c>ClientWebSocket</c> that corresponds to the established connection, if <see cref="Connected"/>.
    /// When not connected, throws an <c>InvalidOperationException</c>.
    /// </summary>
    WebSocket Socket { get; }

    /// <summary>
    /// Gets the common (de)serialiser options to use for this connection
    /// </summary>
    JsonSerializerOptions JsonOptions { get; }

    /// <summary>
    /// Gets a <c>CancellationToken</c> corresponding to the existence of this connection.
    /// Will be created when <see cref="Connect"/> is called, and
    /// cancelled when <see cref="Disconnect"/> is called.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets a value indicating whether a connection has been established.
    /// </summary>
    bool Connected { get; }

    /// <summary>
    /// A helper to assert that a connection exists, throws when that's not the case.
    /// </summary>
    void ThrowIfNotConnected();

    /// <summary>
    /// Registers the API key to use with the Speechmatics RT API.
    /// </summary>
    /// <param name="apiKeyVar">Contains the api key to send to Speechmatics.</param>
    /// <returns>True if the envvar was set and Speechmatics accepts its value, false otherwise.</returns>
    Task<bool> RegisterApiKey(string apiKeyVar);

    /// <summary>
    /// Establishes a connection to Speechmatics.
    /// </summary>
    /// <param name="ct">A CancellationToken to use for the network calls</param>
    /// <returns>Whether or not everything went well</returns>
    Task<bool> Connect(CancellationToken ct);

    /// <summary>
    /// Deestablishes a connection to Speechmatics.
    /// </summary>
    /// <param name="signalSuccess">Whether we should tell Speechmatics that everything went well</param>
    /// <param name="ct">A CancellationToken to use for the network calls</param>
    /// <returns>Whether or not everything went well</returns>
    Task<bool> Disconnect(bool signalSuccess, CancellationToken ct);
}
