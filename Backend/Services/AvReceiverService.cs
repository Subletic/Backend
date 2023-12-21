namespace Backend.Services;

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Services;

/// <summary>
/// A service that fetches new A/V data over a WebSocket and kicks off its transcription via AvProcessingService.
/// </summary>
public class AvReceiverService : IAvReceiverService
{
    /// <summary>
    /// Maximum amount of data to read from the WebSocket at once, in bytes
    /// </summary>
    private const int MAXIMUM_READ_SIZE = 4096;

    /// <summary>
    /// Dependency Injection for AvProcessingService to push fetched data into
    /// </summary>
    private IAvProcessingService avProcessingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvReceiverService"/> class.
    /// </summary>
    /// <param name="avProcessingService">The AvProcessingService to push fetched data into</param>
    public AvReceiverService(IAvProcessingService avProcessingService)
    {
        this.avProcessingService = avProcessingService;
    }

    /// <summary>
    /// Starts the front part of the transcription pipeline (read A/V overWebSocket, push into AvProcessingService)
    /// </summary>
    /// <param name="webSocket">The WebSocket to read A/V data from</param>
    /// <param name="ctSource">The CancellationTokenSource to cancel the operation</param>
    /// <returns> A Task representing the asynchronous operation. </returns>
    public async Task Start(WebSocket webSocket, CancellationTokenSource ctSource)
    {
        Pipe avPipe = new Pipe();
        Stream avWriter = avPipe.Writer.AsStream(leaveOpen: true);
        WebSocketReceiveResult avResult;
        int firstFinishedTask;
        byte[] readBuffer = new byte[MAXIMUM_READ_SIZE];

        Console.WriteLine("Start reading AV data from WebSocket");

        Task<bool> transcriptionTask = avProcessingService.PushProcessedAudio(avPipe.Reader.AsStream(leaveOpen: true));
        List<Task> parallelTasks = new List<Task>
        {
            transcriptionTask,
        };

        do
        {
            Task<WebSocketReceiveResult> readDataTask = webSocket.ReceiveAsync(readBuffer, ctSource.Token);
            parallelTasks.Add(readDataTask);

            Console.WriteLine("Waiting for Client AV data to arrive");
            firstFinishedTask = Task.WaitAny(parallelTasks.ToArray());
            if (firstFinishedTask != parallelTasks.FindIndex(task => task == readDataTask))
                throw new Exception("This shouldn't happen");

            parallelTasks.RemoveAt(firstFinishedTask);
            avResult = await readDataTask;

            if (avResult.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("Received WebSocket close request");
                break;
            }

            // Console.WriteLine("Received data");
            // Console.WriteLine($"Pushing {avResult.Count} bytes into AV pipe");
            var pushAudioDataTask = avWriter.WriteAsync(new ReadOnlyMemory<byte>(readBuffer, 0, avResult.Count), ctSource.Token).AsTask();
            parallelTasks.Add(pushAudioDataTask);

            Console.WriteLine("Waiting for AV data to be pushed for processing");
            firstFinishedTask = Task.WaitAny(parallelTasks.ToArray());
            if (firstFinishedTask != parallelTasks.FindIndex(task => task == pushAudioDataTask))
                throw new Exception("This shouldn't happen");

            parallelTasks.RemoveAt(firstFinishedTask);
            await pushAudioDataTask;
        }
        while (avResult.MessageType != WebSocketMessageType.Close);

        Console.WriteLine("Done reading AV data");

        Console.WriteLine("Waiting for transcription to finish");
        await avPipe.Writer.CompleteAsync();
        bool transcriptionSuccess = await transcriptionTask;
        Console.WriteLine("Transcription " + (transcriptionSuccess ? "success" : "failure"));
    }
}
