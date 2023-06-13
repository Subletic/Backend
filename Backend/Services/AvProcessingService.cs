using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;

using Backend.Controllers;
using Backend.Data;

namespace Backend.Services;

public class AvProcessingService : IAvProcessingService
{
    private static readonly string urlRequestKey = "https://mp.speechmatics.com/v1/api_keys?type=rt";
    private static readonly string urlRecognitionTemplate = "wss://eu2.rt.speechmatics.com/v2/de?jwt={0}";
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly SpeechmaticsStartRecognition_AudioType audioType = new SpeechmaticsStartRecognition_AudioType (
        "raw", "pcm_s16le", 48000);
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        IncludeFields = true,
    };

    /// <summary>
    /// Dependency Injection for accessing the LinkedList of SpeechBubbles and corresponding methods.
    /// Received transcripts are pushed into the speechbubble list
    /// </summary>
    private readonly SpeechBubbleController _speechBubbleController;

    private string? apiKey;
    private ulong sentNum;
    private ulong seqNum;

    public AvProcessingService (SpeechBubbleController speechBubbleController)
    {
        _speechBubbleController = speechBubbleController;
        Console.WriteLine("AvProcessingService is started!");
    }

    private static void logSend (string message)
    {
        Console.WriteLine (String.Format ("Sending to Speechmatics: {0}", message));
    }

    private static void logReceive (string message)
    {
        Console.WriteLine (String.Format ("Received from Speechmatics: {0}", message));
    }

    private static T DeserializeMessage<T> (string buffer, string messageName, string descriptionOfMessage)
    {
        Console.WriteLine ($"Speechmatics sent {descriptionOfMessage}");
        T? messageMaybe = JsonSerializer.Deserialize<T> (buffer, jsonOptions);
        if (messageMaybe is null) throw new InvalidOperationException (
            $"failed to deserialize {messageName} message");
        return (T) messageMaybe;
    }

    // apiKeyVar: envvar that contains the api key to send to speechmatics
    public async Task Init(string apiKeyVar)
    {
        // TODO is it safer to only read a file path to the secret from envvar?
        string? apiKeyEnvMaybe = Environment.GetEnvironmentVariable (apiKeyVar);
        if (apiKeyEnvMaybe == null)
        {
            throw new ArgumentException (String.Format (
                "Requested {0} envvar is not set", apiKeyVar), nameof (apiKeyVar));
        }
        string apiKeyEnv = (string)apiKeyEnvMaybe;

        HttpRequestMessage keyRequest = new HttpRequestMessage (HttpMethod.Post, urlRequestKey);
        keyRequest.Headers.Authorization = new AuthenticationHeaderValue ("Bearer", apiKeyEnv);
        keyRequest.Content = new StringContent ("{\"ttl\": 1200}", Encoding.UTF8, new MediaTypeHeaderValue ("application/json"));

        var keyResponse = await httpClient.SendAsync (keyRequest);
        string keyResponseString = await keyResponse.Content.ReadAsStringAsync();

        if (!keyResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException (String.Format (
                "Speechmatics key request returned unexpected code {0}: {1}",
                keyResponse.StatusCode, keyResponseString));
        }

        // TODO use DeserializeMessage?
        SpeechmaticsKeyResponse? keyResponseParsed = JsonSerializer.Deserialize<SpeechmaticsKeyResponse> (
            keyResponseString, jsonOptions);
        if (keyResponseParsed is null) throw new InvalidOperationException ("failed to deserialize key request response");
        apiKey = ((SpeechmaticsKeyResponse)keyResponseParsed).key_value;

        // FIXME don't print this outside of debugging
        Console.WriteLine ($"Key: {apiKey}");
    }

    // request transcription
    private static async Task<bool> SendStartRecognition (ClientWebSocket wsClient)
    {
        bool success = true;

        try
        {
            // serialisation may fail
            string startRecognitionMessage = JsonSerializer.Serialize (new SpeechmaticsStartRecognition (audioType), jsonOptions);

            logSend (startRecognitionMessage);

            // socket may be closed unexpectedly
            await wsClient.SendAsync (Encoding.UTF8.GetBytes (startRecognitionMessage),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine (e.ToString());
            success = false;
        }

        return success;
    }

    private async Task<bool> ProcessAudioToStream (string filepath, PipeWriter audioPipe,
        SpeechmaticsStartRecognition_AudioType audioType)
    {
        Console.WriteLine ("Started audio processing");
        bool success = true;

        try {
            Action<FFMpegArgumentOptions> outputOptions = options => options
                .WithCustomArgument("-ac 1"); // downmix to mono
            if (audioType.type == "raw") {
                outputOptions += options => options
                    .ForceFormat(audioType.encodingToFFMpegFormat())
                    .WithAudioSamplingRate (audioType.getCheckedSampleRate());
            }
            await FFMpegArguments
                .FromFileInput (filepath, true, options => options
                    .WithDuration(TimeSpan.FromSeconds(60)) // TODO just 10 seconds for now
                )
                .OutputToPipe (new StreamPipeSink (audioPipe.AsStream ()), outputOptions)
                .ProcessAsynchronously();
        } catch (Exception e) {
            // if we don't catch any exceptions in this, the pipe is never marked as completed
            // and the reading side will wait indefinitely
            // TODO propagate exception after pipe flushing & completing for handling?
            Console.WriteLine (e.ToString());
            success = false;
        }

        await audioPipe.FlushAsync();
        await audioPipe.CompleteAsync();
        Console.WriteLine ("Completed audio processing");

        return success;
    }

    private async Task<bool> SendAudio (ClientWebSocket wsClient, string filepath, SpeechmaticsStartRecognition_AudioType audioType)
    {
        Console.WriteLine ("Starting audio sending");

        bool success = true;
        Pipe audioPipe = new Pipe ();
        Stream audioPipeReader = audioPipe.Reader.AsStream (false);
        Task<bool> audioProcessor = ProcessAudioToStream (filepath, audioPipe.Writer, audioType);

        byte[] buffer = new byte[audioType.getCheckedSampleRate() * audioType.bytesPerSample()]; // 1s
        int offset = 0;
        int readCount;
        Console.WriteLine ("Started audio sending");
        try
        {
            do
            {
                // wide range of possible exceptions
                readCount = await audioPipeReader.ReadAsync (buffer.AsMemory (offset, buffer.Length - offset));
                offset += readCount;

                if (readCount != 0)
                {
                    Console.WriteLine (String.Format ("read {0} audio bytes from pipe", readCount));
                }

                bool lastWithLeftovers = readCount == 0 && offset > 0;
                bool shouldSend = (offset == buffer.Length) || lastWithLeftovers;

                if (!shouldSend) continue;

                byte[] sendBuffer = buffer;
                if (lastWithLeftovers) {
                    sendBuffer = new byte[offset];
                    Array.Copy (buffer, 0, sendBuffer, 0, sendBuffer.Length);
                }

                logSend (String.Format ("[{0} bytes of binary audio data]", sendBuffer.Length));

                // socket may be closed unexpectedly
                await wsClient.SendAsync (sendBuffer,
                    WebSocketMessageType.Binary,
                    true,
                    CancellationToken.None);

                sentNum += 1;
                offset = 0;

                // TODO remove when we handle an actual livestream
                // processing a local file is much faster than receiving networked A/V in realtime, simulate the delay
                await Task.Delay (1000);
            } while (readCount != 0);
        }
        catch (Exception e)
        {
            Console.WriteLine (e.ToString());
            success = false;
        }
        Console.WriteLine ("Completed audio sending");

        success &= await audioProcessor;
        Console.WriteLine ("Done sending audio");

        return success;
    }

    private async Task<bool> SendEndOfStream (ClientWebSocket wsClient)
    {
        bool success = true;

        try
        {
            // stop recognition
            // serialisation may fail
            // TODO wait for Speechmatics to stop sending AudioAdded messages before ending the stream so the last_seq_no is correct
            string endOfStreamMessage = JsonSerializer.Serialize(new SpeechmaticsEndOfStream (seqNum), jsonOptions);

            logSend (endOfStreamMessage);

            // socket may be closed unexpectedly
            await wsClient.SendAsync (Encoding.UTF8.GetBytes (endOfStreamMessage),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine (e.ToString());
            success = false;
        }

        return success;
    }

    private async Task<bool> ReceiveMessages (ClientWebSocket wsClient) {
        Console.WriteLine ("Starting message receiving");
        bool success = true;
        bool doneReceivingMessages = false;
        byte[] responseBuffer = new byte[16 * 1024];
        string responseString;

        Console.WriteLine ("Started message receiving");
        try
        {
            while (!doneReceivingMessages) {
                // socket may be closed unexpectedly
                var response = await wsClient.ReceiveAsync (responseBuffer,
                    CancellationToken.None);
                responseString = Encoding.UTF8.GetString (responseBuffer, 0, response.Count);
                logReceive (responseString);

                // TODO do this better
                /* TODO handle missing message types
                 * - Info: diagnostic stuff
                 * - Error: critical errors. recovery is not possible, socket will be closed by Speechmatics. should throw on receival
                 */

                // any of these may throw a deserialisation-related exception

                if (responseString.Contains ("RecognitionStarted")) {
                    SpeechmaticsRecognitionStarted message = DeserializeMessage<SpeechmaticsRecognitionStarted> (responseString,
                        "RecognitionStarted", "a confirmation that it is ready to transcribe our audio");

                    // nothing yet, just nice to have
                }

                if (responseString.Contains ("AudioAdded")) {
                    SpeechmaticsAudioAdded message = DeserializeMessage<SpeechmaticsAudioAdded> (responseString,
                        "AudioAdded", "a confirmation that it received our audio");

                    // TODO inform sending side that Speechmatics is still confirming audio receivals
                    // we don't want to end communication too early
                    seqNum += 1;
                    if (message.seq_no != seqNum) {
                        Console.WriteLine (String.Format (
                            "expected seq_no {0}, received {1} - error? copying received one",
                            seqNum, message.seq_no));
                        seqNum = message.seq_no;
                    }
                }

                if (responseString.Contains ("AddTranscript")) {
                    SpeechmaticsAddTranscript message = DeserializeMessage<SpeechmaticsAddTranscript> (responseString,
                        "AddTranscript", "a transcription of our audio");

                    Console.WriteLine ($"Received transcript: {message.metadata.transcript}");
                    foreach (SpeechmaticsAddTranscript_result transcript in message.results)
                    {
                        _speechBubbleController.HandleNewWord (new WordToken(
                            // docs say this sends a list, I've only ever seen it send 1 result
                            transcript.alternatives[0].content,
                            (float) transcript.alternatives[0].confidence,
                            transcript.start_time,
                            transcript.end_time,
                            // api sends a string?
                            1));
                    }
                }

                if (responseString.Contains ("EndOfTranscript")) {
                    SpeechmaticsEndOfTranscript message = DeserializeMessage<SpeechmaticsEndOfTranscript> (responseString,
                        "EndOfTranscript", "a confirmation that the current transcription process is now done");

                    doneReceivingMessages = true;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine (e.ToString());
            success = false;
        }
        Console.WriteLine ("Completed message receiving");

        return success;
    }

    // TODO handle errors
    public async Task<bool> TranscribeAudio (string filepath) {
        if (apiKey is null)
        {
            Console.WriteLine ("AvProcessingService.Init needs to be called first, we need to have a valid Speechmatics API key!");
            return false;
        }

        bool successSending = true;
        bool successReceiving = true;

        ClientWebSocket wsClient = new ClientWebSocket();
        await wsClient.ConnectAsync (new Uri (String.Format (urlRecognitionTemplate, apiKey)),
            CancellationToken.None);

        // track sent & confirmed audio packet counts
        sentNum = 0;
        seqNum = 0;

        Task<bool> receiveMessages = ReceiveMessages (wsClient);

        successSending = await SendStartRecognition (wsClient);
        if (successSending)
            successSending = await SendAudio (wsClient, filepath, audioType);
        if (successSending)
            successSending = await SendEndOfStream (wsClient);

        successReceiving = await receiveMessages;

        // everything okay
        WebSocketCloseStatus wsCloseStatus = WebSocketCloseStatus.NormalClosure;
        string wsCloseReason = "done";

        if (!successSending || !successReceiving)
        {
            // we've had a problem
            wsCloseStatus = WebSocketCloseStatus.InternalServerError;
            wsCloseReason = "problem with";
            if (!successSending)
                wsCloseReason += " sending";
            if (!successSending && !successReceiving)
                wsCloseReason += " and";
            if (!successReceiving)
                wsCloseReason += " receiving";
        }

        await wsClient.CloseAsync (wsCloseStatus, wsCloseReason, CancellationToken.None);

        return successSending && successReceiving;
    }
}
