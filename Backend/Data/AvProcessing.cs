using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Backend.Data;

public class AvProcessing {
    private static readonly string urlRequestKey = "https://mp.speechmatics.com/v1/api_keys?type=rt";
    private static readonly string urlRecognitionTemplate = "wss://eu2.rt.speechmatics.com/v2/de?jwt={0}";
    private static readonly HttpClient httpClient = new HttpClient();

    private string apiKey;
    private ulong sentNum;
    private ulong seqNum;
    private bool doneSendingAudio;

    private AvProcessing(string apikey) {
        apiKey = apikey;
    }

    private static void logSend (string message) {
        Console.WriteLine (String.Format ("Sending to Speechmatics: {0}", message));
    }

    private static void logReceive (string message) {
        Console.WriteLine (String.Format ("Received from Speechmatics: {0}", message));
    }

    // apiKeyVar: envvar that contains the api key to send to speechmatics
    public static async Task<AvProcessing> Init(string apiKeyVar) {
        // TODO is it safer to only read a file path to the secret from envvar?
        string? apiKeyEnvMaybe = Environment.GetEnvironmentVariable (apiKeyVar);
        if (apiKeyEnvMaybe == null) {
            throw new ArgumentException (String.Format (
                "Requested {0} envvar is not set", apiKeyVar), nameof (apiKeyVar));
        }
        string apiKeyEnv = (string)apiKeyEnvMaybe;

        HttpRequestMessage keyRequest = new HttpRequestMessage (HttpMethod.Post, urlRequestKey);
        keyRequest.Headers.Authorization = new AuthenticationHeaderValue ("Bearer", apiKeyEnv);
        keyRequest.Content = new StringContent ("{\"ttl\": 1200}", Encoding.UTF8, new MediaTypeHeaderValue ("application/json"));

        var keyResponse = await httpClient.SendAsync (keyRequest);
        string keyResponseString = await keyResponse.Content.ReadAsStringAsync();

        if (!keyResponse.IsSuccessStatusCode) {
            throw new InvalidOperationException (String.Format (
                "Speechmatics key request returned unexpected code {0}: {1}",
                keyResponse.StatusCode, keyResponseString));
        }

        SpeechmaticsKeyResponse keyResponseParsed = JsonSerializer.Deserialize<SpeechmaticsKeyResponse> (keyResponseString);
        // FIXME don't print this outside of debugging
        Console.WriteLine ($"Key: {keyResponseParsed.key_value}");

        return new AvProcessing (keyResponseParsed.key_value);
    }

    private async Task ProcessAudioToStream (string filepath, Stream audioPipe) {
        Console.WriteLine ("Started audio processing");
        await FFMpegArguments
            .FromFileInput (filepath, true, options => options
                .WithDuration(TimeSpan.FromSeconds(20)) // TODO just 20 seconds for now
            )
            .OutputToPipe (new StreamPipeSink (audioPipe), options => options
                .ForceFormat("s16le")
                .WithAudioSamplingRate (48000)
                .WithCustomArgument("-ac 1") // mono
            )
            .ProcessAsynchronously();
        audioPipe.Flush();
        audioPipe.Position = 0;
        Console.WriteLine ("Completed audio processing");
    }

    private async Task SendAudio (ClientWebSocket wsClient, string filepath) {
        Console.WriteLine ("Starting audio sending");
        Stream audioPipe = new MemoryStream();
        await ProcessAudioToStream (filepath, audioPipe);

        byte[] buffer = new byte[48000 * 4]; // 1s
        int offset = 0;
        Console.WriteLine ("Started audio sending");
        int readCount;
        do {
            readCount = audioPipe.Read (buffer, offset, buffer.Length - offset);
            offset += readCount;
            Console.WriteLine (String.Format ("read {0} audio bytes from pipe", readCount));

            bool lastWithLeftovers = readCount == 0 && offset > 0;
            bool shouldSend = (offset == buffer.Length) || lastWithLeftovers;

            if (shouldSend) {
                byte[] sendBuffer;
                if (lastWithLeftovers) {
                    sendBuffer = new byte[offset];
                    Array.Copy (buffer, 0, sendBuffer, 0, sendBuffer.Length);
                } else {
                    sendBuffer = buffer;
                }
                logSend (String.Format ("[{0} bytes of binary audio data]", sendBuffer.Length));
                await wsClient.SendAsync (sendBuffer,
                    WebSocketMessageType.Binary,
                    true,
                    CancellationToken.None);
                sentNum += 1;
                offset = 0;
            }
        } while (readCount != 0);
        Console.WriteLine ("Completed audio sending");

        doneSendingAudio = true;
        Console.WriteLine ("Done \"sending\" audio");
    }

    private async Task ReceiveTranscriptions (ClientWebSocket wsClient) {
        Console.WriteLine ("Starting transcription receiving");
        byte[] responseBuffer = new byte[16 * 1024];
        string responseString;

        Console.WriteLine ("Started transcription receiving");
        while (true) {
            var response = await wsClient.ReceiveAsync (responseBuffer,
                CancellationToken.None);
            responseString = Encoding.UTF8.GetString (responseBuffer, 0, response.Count);

            // TODO do this better

            if (responseString.Contains ("AudioAdded")) {
                // TODO check seq_no response
                Console.WriteLine ("Speechmatics confirmed audio receival with AddAudio");
                seqNum += 1;
            }

            if (responseString.Contains ("AddTranscript")) {
                Console.WriteLine ("Speechmatics sent a transcript");
            }

            logReceive (responseString);

            // FIXME AddTranscript's still arrive after the last AudioAdded confirmation
            if (doneSendingAudio && (sentNum == seqNum)) break;
        }
        Console.WriteLine ("Completed transcription receiving");

        Console.WriteLine ("Done \"receiving\" transcriptions");
    }

    // TODO implement better JSON creation / parsing
    public async Task<bool> TranscribeAudio (string filepath) {
        ClientWebSocket wsClient = new ClientWebSocket();
        await wsClient.ConnectAsync (new Uri (String.Format (urlRecognitionTemplate, apiKey)),
            CancellationToken.None);

        // request transcription
        string startRecognitionMessage = @"{
  ""message"": ""StartRecognition"",
  ""audio_format"": {
    ""type"": ""raw"",
    ""encoding"": ""pcm_s16le"",
    ""sample_rate"": 48000
  },
  ""transcription_config"": {
    ""language"": ""de"",
    ""enable_partials"": false
  }
}";

        logSend (startRecognitionMessage);
        await wsClient.SendAsync (Encoding.UTF8.GetBytes (startRecognitionMessage),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        // assuming success
        // TODO handle non-RecognitionStarted message (likely error)
        byte[] responseBuffer = new byte[64 * 1024];
        string responseString;
        var response = await wsClient.ReceiveAsync (responseBuffer,
            CancellationToken.None);
        responseString = Encoding.UTF8.GetString (responseBuffer, 0, response.Count);
        logReceive (responseString);

        // push the data
        sentNum = 0;
        seqNum = 0;
        doneSendingAudio = false;
        Task receiveTranscriptions = ReceiveTranscriptions (wsClient);
        Task sendAudio = SendAudio (wsClient, filepath);

        await sendAudio;
        await receiveTranscriptions;

        // TODO move all the rest of this into ReceiveTranscriptions
        // stop recognition
        string endOfStreamMessageFmt = @"{{
  ""message"": ""EndOfStream"",
  ""last_seq_no"": {0}
}}";
        string endOfStreamMessage = String.Format (endOfStreamMessageFmt, seqNum.ToString());

        logSend (endOfStreamMessage);
        await wsClient.SendAsync (Encoding.UTF8.GetBytes (endOfStreamMessage),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        do {
            // ignore all errors for now, just wait until confirmation
            response = await wsClient.ReceiveAsync (responseBuffer,
                CancellationToken.None);
            responseString = Encoding.UTF8.GetString (responseBuffer, 0, response.Count);
            logReceive (responseString);
        } while (!responseString.Contains ("EndOfTranscript"));

        return true;
    }
}
