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

/**
  * <summary>
  * A service that fetches new A/V data over a WebSocket and kicks off its transcription via AvProcessingService.
  * </summary>
  */
public class AvReceiverService : IAvReceiverService
{
    /**
      * <summary>
      * Maximum amount of data to read from the WebSocket at once, in bytes
      * </summary>
      */
    private const int MAXIMUM_READ_SIZE = 4096;

    /**
      * <summary>
      * Dependency Injection for AvProcessingService to push fetched data into
      * </summary>
      */
    private IAvProcessingService avProcessingService;

    /**
      * <summary>
      * Initializes a new instance of the <see cref="AvReceiverService"/> class.
      * </summary>
      * <param name="avProcessingService">The AvProcessingService to push fetched data into</param>
      */
    public AvReceiverService(IAvProcessingService avProcessingService)
    {
        this.avProcessingService = avProcessingService;
    }

    /**
      * <summary>
      * Starts the front part of the transcription pipeline (read A/V overWebSocket, push into AvProcessingService)
      * </summary>
      * <param name="webSocket">The WebSocket to read A/V data from</param>
      * <param name="ctSource">The CancellationTokenSource to cancel the operation</param>
      * <returns> A Task representing the asynchronous operation. </returns>
      */
    public async Task Start(WebSocket webSocket, CancellationTokenSource ctSource)
    {
        Pipe avPipe = new Pipe();
        Stream avWriter = avPipe.Writer.AsStream(leaveOpen: true);
        WebSocketReceiveResult avResult;
        byte[] readBuffer = new byte[MAXIMUM_READ_SIZE];

        Console.WriteLine("Start reading AV data from WebSocket");

        Task<bool> transcriptionTask = avProcessingService.TranscribeAudio(avPipe.Reader.AsStream(leaveOpen: true));

        do
        {
            avResult = await webSocket.ReceiveAsync(readBuffer, ctSource.Token);

            if (avResult.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("Received WebSocket close request");
                ctSource.Cancel();
                break;
            }

            Console.WriteLine("Received data");

            Console.WriteLine($"Pushing {avResult.Count} bytes into AV pipe");
            await avWriter.WriteAsync(new ReadOnlyMemory<byte>(readBuffer, 0, avResult.Count), ctSource.Token);
        }
        while (avResult.MessageType != WebSocketMessageType.Close);

        Console.WriteLine("Done reading AV data");

        await avPipe.Writer.CompleteAsync();
        bool transcriptionSuccess = await transcriptionTask;
        Console.WriteLine("Transcription " + (transcriptionSuccess ? "success" : "failure"));
    }
}
