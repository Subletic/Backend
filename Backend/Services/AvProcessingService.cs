namespace Backend.Services;

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
using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage;
using Backend.Data.SpeechmaticsMessages.AddTranscriptMessage.result;
using Backend.Data.SpeechmaticsMessages.AudioAddedMessage;
using Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;
using Backend.Data.SpeechmaticsMessages.EndOfTranscriptMessage;
using Backend.Data.SpeechmaticsMessages.ErrorMessage;
using Backend.Data.SpeechmaticsMessages.InfoMessage;
using Backend.Data.SpeechmaticsMessages.RecognitionStartedMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;
using Backend.Data.SpeechmaticsMessages.WarningMessage;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

/// <summary>
/// Service that takes some A(/V) stream, runs its audio against the Speechmatics realtime API
/// and pushes the received transcripts into the <c>SpeechBubbleController</c> for storage
/// and the Backend <-> Frontend data exchange.
/// <example>
/// For example:
/// <code>
/// AvProcessingService avp = new AvProcessingService();
/// bool canUse = await avp.Init ("NAME_OF_ENVVAR_WITH_API_KEY");
/// if (canUse)
/// {
///     Task<bool> audioTranscription = avp.TranscribeAudio ("/path/to/media.file");
///     // do other things
///     bool atSuccess = await audioTranscription;
/// }
/// </code>
/// will initialise the service with your personal Speechmatics API key, and run some media file
/// through the RT API.
/// </example>
/// </summary>
public class AvProcessingService : IAvProcessingService
{
    /// <summary>
    /// Dependency Injection to get the queue via which <c>CommunicationHub.ReceiveAudioStream</c>
    /// will send audio buffers to the frontend.
    /// <see cref="CommunicationHub" />
    /// </summary>
    private readonly IFrontendCommunicationService frontendCommunicationService;

    private readonly ISpeechmaticsConnectionService speechmaticsConnectionService;

    private readonly ISpeechmaticsSendService speechmaticsSendService;

    private readonly Serilog.ILogger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvProcessingService"/> class.
    /// </summary>
    /// <param name="wordProcessingService">The <c>SpeechBubbleController</c> to push new words into</param>
    /// <param name="frontendCommunicationService">The Audio to push new audio into for the Frontend</param>
    public AvProcessingService(
        IFrontendCommunicationService frontendCommunicationService,
        ISpeechmaticsConnectionService speechmaticsConnectionService,
        ISpeechmaticsSendService speechmaticsSendService,
        Serilog.ILogger log)
    {
        this.frontendCommunicationService = frontendCommunicationService;
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.speechmaticsSendService = speechmaticsSendService;
        this.log = log;
    }

    /// <summary>
    /// Uses FFMpeg to process a file into the required audio format and push the data
    /// into the input side of a <c>Pipe</c>.
    /// Internally launched and <c>await</c>ed by <c>SendAudio</c>.
    /// At the end of the processing, no matter whether or not an error occurred,
    /// the <paramref name="audioPipe" /> is always flushed and closed.
    /// To not exhaust our API keys during development, we only push up to 1 minute of audio into the pipe.
    /// <param name="mediaUri">A URI to some media to run through FFMpeg.</param>
    /// <param name="audioPipe">A <c>PipeWriter</c> to push the data into.</param>
    /// <returns>
    /// An <c>await</c>able <c>Task{bool}</c> indicating if the processing went well.
    /// </returns>
    /// <seealso cref="sendAudio" />
    /// <seealso cref="TranscribeAudio" />
    /// </summary>
    private async Task<bool> processAudioToStream(Stream avStream, PipeWriter audioPipe)
    {
        log.Debug("Started audio processing with FFmpeg");
        bool success = true;

        try
        {
            Action<FFMpegArgumentOptions> outputOptions = options => options
                .WithCustomArgument("-ac 1") // downmix stereo audio to mono
                .WithCustomArgument("-vn") // throw away video streams
                .WithCustomArgument("-sn"); // throw away subtitle streams
            if (speechmaticsConnectionService.AudioFormat.type == "raw")
            {
                outputOptions += options => options
                    .ForceFormat(speechmaticsConnectionService.AudioFormat.GetEncodingInFFMpegFormat())
                    .WithAudioSamplingRate(speechmaticsConnectionService.AudioFormat.GetCheckedSampleRate());
            }

            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(avStream))
                .OutputToPipe(new StreamPipeSink(audioPipe.AsStream()), outputOptions)
                .ProcessAsynchronously();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            success = false;
        }

        // always flush & mark complete, so other side of pipe can move on
        await audioPipe.FlushAsync();
        await audioPipe.CompleteAsync();
        log.Debug("Completed audio processing with FFmpeg");

        return success;
    }

    /// <summary>
    /// Accumulates the data from <c>processAudioToStream</c> and sends buffers of a suitable size
    /// to the Speechmatics RT API for recognition and transcription.
    /// Internally launches and <c>await</c>s <c>ProcessAudioToStream</c>.
    /// <param name="wsClient">A <c>ClientWebSocket</c> to send the <c>AddAudio</c> messages (the buffers) over.</param>
    /// <param name="mediaUri">A URI of some media to run through FFMpeg.</param>
    /// <returns>
    /// An <c>await</c>able <c>Task{bool}</c> indicating if the processing and sending went well.
    /// </returns>
    /// <seealso cref="processAudioToStream" />
    /// <seealso cref="TranscribeAudio" />
    /// </summary>
    public async Task<bool> PushProcessedAudio(Stream avStream)
    {
        log.Debug("Starting audio pushing");

        bool success = true;
        Pipe audioPipe = new Pipe();
        Stream audioPipeReader = audioPipe.Reader.AsStream(false);
        Task<bool> audioProcessor = processAudioToStream(avStream, audioPipe.Writer);
        List<Task> parallelTasks = new List<Task>
        {
            audioProcessor,
        };

        int offset = 0;
        int readCount;
        int firstFinishedTask;

        try
        {
            byte[] buffer = new byte[speechmaticsConnectionService.AudioFormat.GetCheckedSampleRate() * speechmaticsConnectionService.AudioFormat.GetBytesPerSample()]; // 1s
            log.Information("Started audio pushing");
            do
            {
                // wide range of possible exceptions
                Task<int> fetchProcessedAudioTask = audioPipeReader.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset)).AsTask();
                parallelTasks.Add(fetchProcessedAudioTask);

                int fetchTaskIndex;
                do
                {
                    firstFinishedTask = Task.WaitAny(parallelTasks.ToArray());
                    fetchTaskIndex = parallelTasks.FindIndex(x => x == fetchProcessedAudioTask);
                }
                while (firstFinishedTask != fetchTaskIndex);

                readCount = await fetchProcessedAudioTask;
                offset += readCount;

                if (readCount != 0)
                    log.Debug($"read {readCount} audio bytes from pipe");

                bool lastWithLeftovers = readCount == 0 && offset > 0;
                bool shouldSend = (offset == buffer.Length) || lastWithLeftovers;

                if (!shouldSend) continue;

                byte[] sendBuffer = buffer;
                if (lastWithLeftovers)
                {
                    sendBuffer = new byte[offset];
                    Array.Copy(buffer, 0, sendBuffer, 0, sendBuffer.Length);
                }

                // push to speechmatics
                await speechmaticsSendService.SendAudio(sendBuffer);

                // store only decoded audio
                short[] storeShortBuffer = new short[buffer.Length / 2];
                Buffer.BlockCopy(sendBuffer, 0, storeShortBuffer, 0, (sendBuffer.Length / 2) * 2);

                // play back with zero padding
                if (lastWithLeftovers)
                {
                    sendBuffer = new byte[buffer.Length];
                    Array.Copy(buffer, 0, sendBuffer, 0, buffer.Length);
                }

                short[] sendShortBuffer = new short[speechmaticsConnectionService.AudioFormat.GetCheckedSampleRate()];
                Buffer.BlockCopy(sendBuffer, 0, sendShortBuffer, 0, sendBuffer.Length);
                frontendCommunicationService.Enqueue(sendShortBuffer);

                // TODO remove when we handle an actual livestream
                // processing a local file is much faster than receiving networked A/V in realtime, simulate the delay
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            while (readCount != 0);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            success = false;
        }

        log.Debug("Completed audio pushing");

        success = success && await audioProcessor;
        log.Information("Done pushing audio");

        return success;
    }
}
