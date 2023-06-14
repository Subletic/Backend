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
using System.Text.RegularExpressions;
using System.Threading;

using Backend.Controllers;
using Backend.Data;
using Backend.Data.SpeechmaticsMessages;

namespace Backend.Services;

/**
  *  <summary>
  *  Service that takes some A(/V) stream, runs its audio against the Speechmatics realtime API
  *  and pushes the received transcripts into the <c>SpeechBubbleController</c> for storage
  *  and the Backend <-> Frontend data exchange.
  *  <example>
  *  For example:
  *  <code>
  *  AvProcessingService avp = new AvProcessingService();
  *  bool canUse = await avp.Init ("NAME_OF_ENVVAR_WITH_API_KEY");
  *  if (canUse)
  *  {
  *      Task<bool> audioTranscription = avp.TranscribeAudio ("/path/to/media.file");
  *      // do other things
  *      bool atSuccess = await audioTranscription;
  *  }
  *  </code>
  *  will initialise the service with your personal Speechmatics API key, and run some media file
  *  through the RT API.
  *  </example>
  *  </summary>
  */
public partial class AvProcessingService : IAvProcessingService
{
    /**
      *  <summary>
      *  The Speechmatics URL we'll send the API key to in order to request a RT API access token.
      *  </summary>
      */
    private static readonly string urlRequestKey = "https://mp.speechmatics.com/v1/api_keys?type=rt";

    /**
      *  <summary>
      *  A URL template for the Speechmatics RT API.
      *  The received RT API access token shall be filled in to get the URL we'll need to use.
      *  </summary>
      */
    private static readonly string urlRecognitionTemplate = "wss://eu2.rt.speechmatics.com/v2/de?jwt={0}";

    /**
      *  <summary>
      *  A description of the audio format we'll send to the RT API.
      *  <see cref="StartRecognitionMessage_AudioType" />
      *  </summary>
      */
    private static readonly StartRecognitionMessage_AudioType audioType =
        new StartRecognitionMessage_AudioType ("raw", "pcm_s16le", 48000);

    /**
      *  <summary>
      *  Options to use for all Object -> JSON serialisations
      *  <see cref="JsonSerializerOptions" />
      *  </summary>
      */
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        IncludeFields = true,
    };

    /**
      *  <summary>
      *  A regular expression to identify what type of message Speechmatics sent us.
      *  Based on this captured type string, the message will get deserialised to an object
      *  of the corresponding message class.
      *  Matches: <c>"message" : "(SomeMessageType)"</c>
      *  </summary>
      */
    [GeneratedRegex(@"""message""\s*:\s*""([^""]+)""")]
    private static partial Regex messageTypeRegex();

    /**
      *  <summary>
      *  Dependency Injection to get an instance of the <c>SpeechBubbleController</c>.
      *  This is needed to call its <c>HandleNewWord</c> method, to push words from the received
      *  transcript messages into our system.
      *  <see cref="SpeechBubbleController.HandleNewWord" />
      *  </summary>
      */
    private readonly SpeechBubbleController _speechBubbleController;

    /**
      *  <summary>
      *  The Speechmatics RT API key this instance shall use for the RT transcription.
      *  <see cref="Init" />
      *  </summary>
      */
    private string? apiKey;

    /**
      *  <summary>
      *  A tracker for the number of <c>AddAudioMessage</c>s this instance has sent to the RT API.
      *  This is used to identify how many <c>AudioAddedMessage</c>s we should await from the API
      *  before we should properly terminate the line of communication.
      *  <see cref="seqNum" />
      *  <see cref="AddAudioMessage" />
      *  <see cref="AudioAddedMessage" />
      */
    private ulong sentNum;

    /**
      *  <summary>
      *  A tracker for the number of <c>AudioAddedMessage</c>s this instance has received from the RT API.
      *  This is used to identify how many <c>AudioAddedMessage</c>s we should await from the API
      *  before we should properly terminate the line of communication.
      *
      *  Additionally, the final <c>EndOfStreamMessage</c> we use to terminate the transcription
      *  must include the sequence number of the last <c>AddedaudioMessage</c> we've received from the API.
      *  <see cref="sentNum" />
      *  <see cref="AudioAddedMessage" />
      *  <see cref="EndOfStreamMessage" />
      *  </summary>
      */
    private ulong seqNum;

    /**
      *  <summary>
      *  Constructor of the service.
      *  <param name="speechBubbleController">The <c>SpeechBubbleController</c> to push new words into</param>
      *  </summary>
      */
    public AvProcessingService (SpeechBubbleController speechBubbleController)
    {
        _speechBubbleController = speechBubbleController;
        Console.WriteLine("AvProcessingService is started!");
    }

    /**
      *  <summary>
      *  Log outgoing RT API message.
      *  <see cref="logReceive" />
      *  </summary>
      */
    private static void logSend (string message)
    {
        Console.WriteLine ($"Sending to Speechmatics: {message}");
    }

    /**
      *  <summary>
      *  Log incoming RT API message.
      *  <see cref="logSend" />
      *  </summary>
      */
    private static void logReceive (string message)
    {
        Console.WriteLine ($"Received from Speechmatics: {message}");
    }

    /**
      *  <summary>
      *  Attempt to deserialise the message in <paramref name="buffer" /> into a Message type.
      *  The caller can add a <paramref name="messageName" /> and <paramref name="descriptionOfMessage" />
      *  for logging purposes.
      *
      *  <typeparam name="T">The message class to deserialise the message into.</typeparam>
      *
      *  <param name="buffer">A <c>string</c> buffer that holds a JSON message.</param>
      *  <param name="messageName">A pretty name for the message, for logging.</param>
      *  <param name="descriptionOfMessage">A pretty description for what the message is for, for logging.</param>
      *
      *  <returns>An instance of the requested message class</returns>
      *
      *  <exception cref="InvalidOperationException">
      *  <c>JsonSerializer.Deserialize{T}</c> returned <c>null</c>. Microsoft documentation does not indicate why this
      *  would happen, I'm assuming when <paramref name="buffer" /> is <c>"null"</c>?
      *  </exception>
      *  <exception cref="ArgumentNullException">Passed through from <c>JsonSerializer.Deserialize{T}</c></exception>
      *  <exception cref="JsonException">Passed through from <c>JsonSerializer.Deserialize{T}</c></exception>
      *  <exception cref="NotSupportedException">Passed through from <c>JsonSerializer.Deserialize{T}</c></exception>
      *  <see cref="System.Text.Json.JsonSerializer.Deserialize{T}" />
      *  </summary>
      */
    private static T DeserializeMessage<T> (string buffer, string messageName = typeof(T).ToString(),
        string descriptionOfMessage = $"a message of type {messageName}")
    {
        T? messageMaybe = JsonSerializer.Deserialize<T> (buffer, jsonOptions);
        if (messageMaybe is null)
            throw new InvalidOperationException ($"Failed to deserialize {messageName} message "
                + $"into type {typeof(T).ToString()}");

        Console.WriteLine ($"Speechmatics sent {descriptionOfMessage}");
        return messageMaybe!;
    }

    /**
      *  <summary>
      *  Initialise the RT API access token held in <c>apiKey</c>.
      *
      *  <param name="apiKeyVar">The name of the environment variable that holds your API key.</param>
      *
      *  <returns>
      *  An <c>await</c>able <c>Task{bool}</c> indicating if the envvar was set and we got an RT API access token.
      *  </returns>
      *
      *  <exception cref="InvalidOperationException">
      *  1. If the request to Speechmatics returned an unexpected (unsuccessful) status code.
      *  2. If the response from Speechmatics deserialised into <c>null</c>.
      *  <see cref="DeserializeMessage{T}" />
      *  </exception>
      *
      *  <see cref="apiKey" />
      *  </summary>
      */
    public async Task<bool> Init(string apiKeyVar)
    {
        // TODO is it safer to only read a file path to the secret from envvar?
        string? apiKeyEnvMaybe = Environment.GetEnvironmentVariable (apiKeyVar);
        if (apiKeyEnvMaybe == null)
        {
            Console.WriteLine ($"Requested {apiKeyVar} envvar is not set");
            return false;
        }
        string apiKeyEnv = apiKeyEnvMaybe!;

        HttpRequestMessage keyRequest = new HttpRequestMessage (HttpMethod.Post, urlRequestKey);
        keyRequest.Headers.Authorization = new AuthenticationHeaderValue ("Bearer", apiKeyEnv);
        keyRequest.Content = new StringContent (
            // TODO don't manually hack together JSON like this
            "{\"ttl\": 1200}",
            Encoding.UTF8,
            new MediaTypeHeaderValue ("application/json"));

        var keyResponse = await new HttpClient().SendAsync (keyRequest);
        string keyResponseString = await keyResponse.Content.ReadAsStringAsync();

        if (!keyResponse.IsSuccessStatusCode)
        {
            // TODO maybe just log and return false instead
            throw new InvalidOperationException (String.Format (
                "Speechmatics key request returned unexpected code {0}: {1}",
                keyResponse.StatusCode, keyResponseString));
        }

        // TODO use DeserializeMessage?
        TemporaryKeyResponse? keyResponseParsed = JsonSerializer.Deserialize<TemporaryKeyResponse> (
            keyResponseString, jsonOptions);
        if (keyResponseParsed is null)
            throw new InvalidOperationException ("Failed to deserialize key request response");
        apiKey = keyResponseParsed!.key_value;

        // FIXME don't print this outside of debugging
        Console.WriteLine ($"Key: {apiKey}");
        return true;
    }

    /**
      *  <summary>
      *  Sends a <c>StartRecognitionMessage</c> message to the RT API.
      *  This will start the recognition and transcription process on the server.
      *
      *  <param name="wsClient">A <c>ClientWebSocket</c> to send the message over.</param>
      *
      *  <returns>
      *  An <c>await</c>able <c>Task{bool}</c> indicating if the serialisation and transmission went well.
      *  </returns>
      *
      *  <see cref="StartRecognitionMessage" />
      *  <seealso cref="TranscribeAudio" />
      *  </summary>
      */
    private static async Task<bool> SendStartRecognition (ClientWebSocket wsClient)
    {
        bool success = true;

        try
        {
            // serialisation may fail
            string startRecognitionMessage = JsonSerializer.Serialize (new StartRecognitionMessage (audioType),
                jsonOptions);

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

    /**
      *  <summary>
      *  Uses FFMpeg to process a file into the required audio format and push the data
      *  into the input side of a <c>Pipe</c>.
      *
      *  Internally launched and <c>await</c>ed by <c>SendAudio</c>.
      *
      *  At the end of the processing, no matter whether or not an error occurred,
      *  the <paramref name="audioPipe" /> is always flushed and closed.
      *
      *  To not exhaust our API keys during development, we only push up to 1 minute of audio into the pipe.
      *
      *  <param name="filepath">A path to a media file to run through FFMpeg.</param>
      *  <param name="audioPipe">A <c>PipeWriter</c> to push the data into.</param>
      *
      *  <returns>
      *  An <c>await</c>able <c>Task{bool}</c> indicating if the processing went well.
      *  </returns>
      *
      *  <seealso cref="SendAudio" />
      *  <seealso cref="TranscribeAudio" />
      *  </summary>
      */
    private async Task<bool> ProcessAudioToStream (string filepath, PipeWriter audioPipe)
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
                    .WithDuration(TimeSpan.FromSeconds(60)) // TODO just 1 minute for now
                )
                .OutputToPipe (new StreamPipeSink (audioPipe.AsStream ()), outputOptions)
                .ProcessAsynchronously();
        } catch (Exception e) {
            Console.WriteLine (e.ToString());
            success = false;
        }

        // always flush & mark complete, so other side of pipe can move on
        await audioPipe.FlushAsync();
        await audioPipe.CompleteAsync();
        Console.WriteLine ("Completed audio processing");

        return success;
    }

    /**
      *  <summary>
      *  Accumulates the data from <c>ProcessAudioToStream</c> and sends buffers of a suitable size
      *  to the Speechmatics RT API for recognition and transcription.
      *
      *  Internally launches and <c>await</c>s <c>ProcessAudioToStream</c>.
      *
      *  <param name="wsClient">A <c>ClientWebSocket</c> to send the <c>AddAudio</c> messages (the buffers) over.</param>
      *  <param name="filepath">A path to a media file to run through FFMpeg.</param>
      *
      *  <returns>
      *  An <c>await</c>able <c>Task{bool}</c> indicating if the processing and sending went well.
      *  </returns>
      *
      *  <seealso cref="ProcessAudioToStream" />
      *  <seealso cref="TranscribeAudio" />
      *  </summary>
      */
    private async Task<bool> SendAudio (ClientWebSocket wsClient, string filepath)
    {
        Console.WriteLine ("Starting audio sending");

        bool success = true;
        Pipe audioPipe = new Pipe ();
        Stream audioPipeReader = audioPipe.Reader.AsStream (false);
        Task<bool> audioProcessor = ProcessAudioToStream (filepath, audioPipe.Writer, audioType);

        int offset = 0;
        int readCount;

        try
        {
            byte[] buffer = new byte[audioType.getCheckedSampleRate() * audioType.bytesPerSample()]; // 1s
            Console.WriteLine ("Started audio sending");
            do
            {
                // wide range of possible exceptions
                readCount = await audioPipeReader.ReadAsync (buffer.AsMemory (offset, buffer.Length - offset));
                offset += readCount;

                if (readCount != 0)
                    Console.WriteLine ($"read {readCount} audio bytes from pipe");

                bool lastWithLeftovers = readCount == 0 && offset > 0;
                bool shouldSend = (offset == buffer.Length) || lastWithLeftovers;

                if (!shouldSend) continue;

                byte[] sendBuffer = buffer;
                if (lastWithLeftovers) {
                    sendBuffer = new byte[offset];
                    Array.Copy (buffer, 0, sendBuffer, 0, sendBuffer.Length);
                }

                logSend ($"[{sendBuffer.Length} bytes of binary audio data]");

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

        success = success && await audioProcessor;
        Console.WriteLine ("Done sending audio");

        return success;
    }

    /**
      *  <summary>
      *  Sends an <c>EndOfStreamMessage</c> message to the RT API.
      *  This will end the recognition and transcription process on the server.
      *
      *  <param name="wsClient">A <c>ClientWebSocket</c> to send the message over.</param>
      *
      *  <returns>
      *  An <c>await</c>able <c>Task{bool}</c> indicating if the serialisation and transmission went well.
      *  </returns>
      *
      *  <see cref="EndOfStreamMessage" />
      *  </summary>
      */
    private async Task<bool> SendEndOfStream (ClientWebSocket wsClient)
    {
        bool success = true;

        try
        {
            // serialisation may fail
            string endOfStreamMessage = JsonSerializer.Serialize(new EndOfStreamMessage (seqNum), jsonOptions);

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

    /**
      *  <summary>
      *  Listens for and acts upon messages from the RT API.
      *  All sorts of messages from the <c>Backend.Data.SpeechmaticsMessages</c> namespace can be received and handled.
      *
      *  <param name="wsClient">A <c>ClientWebSocket</c> to receive messages over.</param>
      *
      *  <returns>
      *  An <c>await</c>able <c>Task{bool}</c> indicating if the receiving and deserialisations went well,
      *  no unknown messages were received and the RT API never reported any problems.
      *  </returns>
      *  </summary>
      */
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

                MatchCollection messageMatches = messageTypeRegex().Matches (responseString);
                if (messageMatches.Count != 1)
                    throw new InvalidOperationException (
                        $"Found unexpected amount of message type matches: {messageMatches.Count}");

                // any of these may throw a deserialisation-related exception
                // TODO factor all of these out into separate methods
                switch (messageMatches[0].Groups[1].ToString())
                {
                    case "Error":
                        ErrorMessage errorMessage = DeserializeMessage<ErrorMessage> (responseString,
                            "Error", "a critical error");

                        // the server has stopped the transcription and will close the connection. propagate its error
                        throw new Exception ($"{errorMessage.type}: {errorMessage.reason}");

                    case "Warning":
                        WarningMessage warningMessage = DeserializeMessage<WarningMessage> (responseString,
                            "Warning", "a warning");

                        // nothing yet, just nice to have
                        break;

                    case "Info":
                        InfoMessage infoMessage = DeserializeMessage<InfoMessage> (responseString,
                            "Info", "additional information");

                        // nothing yet, just nice to have
                        break;

                    case "RecognitionStarted":
                        RecognitionStartedMessage rsMessage = DeserializeMessage<RecognitionStartedMessage> (
                            responseString, "RecognitionStarted",
                            "a confirmation that it is ready to transcribe our audio");

                        // nothing yet, just nice to have
                        break;

                    case "AudioAdded":
                        AudioAddedMessage aaMessage = DeserializeMessage<AudioAddedMessage> (responseString,
                            "AudioAdded", "a confirmation that it received our audio");

                        // TODO inform sending side of this class that Speechmatics is still confirming audio receivals
                        // we don't want to end communication too early
                        seqNum += 1;
                        if (aaMessage.seq_no != seqNum) {
                            Console.WriteLine (String.Format (
                                "expected seq_no {0}, received {1} - error? copying received one",
                                seqNum, aaMessage.seq_no));
                            seqNum = aaMessage.seq_no;
                        }
                        break;

                    case "AddTranscript":
                        AddTranscriptMessage atMessage = DeserializeMessage<AddTranscriptMessage> (responseString,
                            "AddTranscript", "a transcription of our audio");

                        Console.WriteLine ($"Received transcript: {atMessage.metadata.transcript}");

                        foreach (AddTranscriptMessage_result transcript in atMessage.results!)
                        {
                            // the specs say an AddTranscript.results may come without an alternatives list.
                            // TODO what is its purpose?
                            if (transcript.alternatives is null)
                                throw new InvalidOperationException (
                                    "Received a transcript result without an alternatives list. "
                                    + "Specifications say this is a possibility, but what is its purpose? "
                                    + $"Analyse: {responseString}");

                            _speechBubbleController.HandleNewWord (new WordToken(
                                // docs say this sends a list, I've only ever seen it send 1 result
                                transcript.alternatives![0].content,
                                (float) transcript.alternatives![0].confidence,
                                transcript.start_time,
                                transcript.end_time,
                                // TODO api sends a string, extract a number from it
                                // https://docs.speechmatics.com/features/diarization#speaker-diarization
                                // speaker identified: "S<speaker-id>"
                                // not identified: "UU"
                                1));
                        }
                        break;

                   case "EndOfTranscript":
                        EndOfTranscriptMessage eotMessage = DeserializeMessage<EndOfTranscriptMessage> (responseString,
                            "EndOfTranscript", "a confirmation that the current transcription process is now done");

                        doneReceivingMessages = true;
                        break;

                   default:
                        throw new Exception ($"Unknown Speechmatics message: {responseString}");
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
            Console.WriteLine ("Valid Speechmatics API key required, call AvProcessingService.Init first");
            return false;
        }

        bool successSending = true;
        bool successReceiving = true;

        ClientWebSocket wsClient = new ClientWebSocket();
        await wsClient.ConnectAsync (
            new Uri (String.Format (urlRecognitionTemplate, apiKey)),
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
