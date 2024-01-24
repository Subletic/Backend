namespace Backend.Services;

using System.IO.Pipelines;
using FFMpegCore;
using FFMpegCore.Pipes;
using ILogger = Serilog.ILogger;

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

    private readonly IConfiguration configuration;

    private readonly ILogger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvProcessingService"/> class.
    /// </summary>
    /// <param name="frontendCommunicationService">The service to push new audio into the Frontend</param>
    /// <param name="speechmaticsConnectionService">The service to get audio format details for the communication with Speechmatics</param>
    /// <param name="speechmaticsSendService">The service to push new audio into Speechmatics</param>
    /// <param name="configuration">DI appsettings reference</param>
    /// <param name="log">The logger</param>
    public AvProcessingService(
        IFrontendCommunicationService frontendCommunicationService,
        ISpeechmaticsConnectionService speechmaticsConnectionService,
        ISpeechmaticsSendService speechmaticsSendService,
        IConfiguration configuration,
        ILogger log)
    {
        this.frontendCommunicationService = frontendCommunicationService;
        this.speechmaticsConnectionService = speechmaticsConnectionService;
        this.speechmaticsSendService = speechmaticsSendService;
        this.configuration = configuration;
        this.log = log;
    }

    /// <summary>
    /// Uses FFMpeg to process a file into the required audio format and push the data
    /// into the input side of a <c>Pipe</c>.
    /// Internally launched and <c>await</c>ed by <c>PushProcessedAudio</c>.
    /// At the end of the processing, no matter whether or not an error occurred,
    /// the <paramref name="audioPipe" /> is always flushed and closed.
    /// </summary>
    /// <param name="avStream">A Stream to read media data from.</param>
    /// <param name="audioPipe">A <c>PipeWriter</c> to push the data into.</param>
    /// <param name="ctSource">Reference to to CT Source to cancel all other services in case something goes wrong</param>
    /// <returns>
    /// An <c>await</c>able <c>Task{bool}</c> indicating if the processing went well.
    /// </returns>
    /// <seealso cref="PushProcessedAudio" />
    private async Task<bool> processAudioToStream(Stream avStream, PipeWriter audioPipe, CancellationTokenSource ctSource)
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
            log.Error($"Failed to process AV data with FFmpeg: {e.Message}");
            log.Debug(e.ToString());
            await frontendCommunicationService.AbortCorrection($"Fehler bei der Verarbeitung der Echtzeitübertragung: {e.Message}");
            success = false;
            ctSource.Cancel();
        }

        // always flush & mark complete, so other side of pipe can move on
        await audioPipe.FlushAsync();
        await audioPipe.CompleteAsync();
        log.Debug("Completed audio processing with FFmpeg");

        return success;
    }

    private async Task<int> readProcessedChunk(Stream processedAudioPipe, byte[] bufferToFill)
    {
        int offset = 0;
        int readCount = 0;
        do
        {
            readCount = await processedAudioPipe.ReadAsync(
                bufferToFill.AsMemory(offset, bufferToFill.Length - offset));
            offset += readCount;

            if (readCount != 0)
                log.Debug($"Read {readCount} bytes of processed audio");
        }
        while ((offset != bufferToFill.Length) && (readCount != 0));

        return offset;
    }

    private async Task sendAudioToSpeechmatics(byte[] buffer, int filledAmount)
    {
        // don't send any zero-padding, waste of data
        byte[] sendBuffer = buffer;
        if (filledAmount != buffer.Length)
        {
            sendBuffer = new byte[filledAmount];
            Array.Copy(buffer, 0, sendBuffer, 0, sendBuffer.Length);
        }

        // push to speechmatics
        await speechmaticsSendService.SendAudio(sendBuffer);
    }

    private void sendAudioToFrontend(byte[] buffer)
    {
        short[] frontendBuffer = new short[speechmaticsConnectionService.AudioFormat.GetCheckedSampleRate()];
        Buffer.BlockCopy(buffer, 0, frontendBuffer, 0, buffer.Length);
        frontendCommunicationService.Enqueue(frontendBuffer);
    }

    /// <summary>
    /// Accumulates the data from <c>processAudioToStream</c> and sends buffers of a suitable size
    /// to the Speechmatics RT API for recognition and transcription, and the frontend for local playback.
    /// Internally launches and <c>await</c>s <c>ProcessAudioToStream</c>.
    /// <seealso cref="processAudioToStream" />
    /// </summary>
    /// <param name="avStream">A Stream to read media data from.</param>
    /// <param name="ctSource">Reference to to CT Source to cancel all other services in case something goes wrong</param>
    /// <returns>An <c>await</c>able <c>Task{bool}</c> indicating if the processing and sending went well.</returns>
    public async Task<bool> PushProcessedAudio(Stream avStream, CancellationTokenSource ctSource)
    {
        log.Debug("Starting audio pushing");

        bool success = true;
        Pipe audioPipe = new Pipe();
        Stream audioPipeReader = audioPipe.Reader.AsStream(false);
        Task<bool> audioProcessor = processAudioToStream(avStream, audioPipe.Writer, ctSource);

        bool simulateDelay = configuration.GetValue<bool>("ClientCommunicationSettings:SIMULATE_LIVESTREAM_RATE");

        try
        {
            byte[] buffer = new byte[speechmaticsConnectionService.AudioFormat.GetCheckedSampleRate() * speechmaticsConnectionService.AudioFormat.GetBytesPerSample()]; // 1s
            int filledAmount = 0;
            log.Debug("Started audio processing");

            while ((filledAmount = await readProcessedChunk(audioPipeReader, buffer)) != 0)
            {
                try
                {
                    ctSource.Token.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    log.Information("Cancellation of processed audio pushing has been requested");
                    throw;
                }

                Task sendToSpeechmatics = sendAudioToSpeechmatics(buffer, filledAmount);
                sendAudioToFrontend(buffer);
                await sendToSpeechmatics;

                // processing a local file via the mock-server is much faster than receiving a real livestream, simulate the delay
                if (simulateDelay)
                    await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
        catch (OperationCanceledException)
        {
            log.Information("Pushing processed audio has been cancelled");
        }
        catch (Exception e)
        {
            log.Error($"Failed to push processed audio from FFmpeg to Speechmatics: {e.Message}");
            log.Debug(e.ToString());
            success = false;
            await frontendCommunicationService.AbortCorrection($"Fehler beim Lesen der verarbeiteten Echtzeitübertragung: {e.Message}");
            ctSource.Cancel();
        }

        log.Debug("Awaiting FFmpeg to finish");
        success = success && await audioProcessor;

        log.Information("Done processing audio");
        return success;
    }
}
