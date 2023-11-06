using Backend.Data;
using Backend.Services;

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Services;

public class AvReceiverService : IAvReceiverService
{
    private const int MAXIMUM_READ_SIZE = 4096;

    private IAvProcessingService avProcessingService;

    public AvReceiverService(IAvProcessingService avProcessingService)
    {
        this.avProcessingService = avProcessingService;
    }

    public async Task Start(WebSocket webSocket, CancellationTokenSource ctSource)
    {
        Pipe avPipe = new Pipe();
        Stream avWriter = avPipe.Writer.AsStream (leaveOpen: true);
        WebSocketReceiveResult avResult;
        byte[] readBuffer = new byte[MAXIMUM_READ_SIZE];

        Console.WriteLine ("Start reading AV data from WebSocket");

        Task<bool> transcriptionTask = avProcessingService.TranscribeAudio (avPipe.Reader.AsStream (leaveOpen: true));

        do
        {
            avResult = await webSocket.ReceiveAsync (readBuffer,
                ctSource.Token);

            if (avResult.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine ("Received WebSocket close request");
                ctSource.Cancel();
                break;
            }

            Console.WriteLine ("Received data");

            Console.WriteLine ($"Pushing {avResult.Count} bytes into AV pipe");
            await avWriter.WriteAsync (new ReadOnlyMemory<byte> (readBuffer, 0, avResult.Count), ctSource.Token);
        }
        while (avResult.MessageType != WebSocketMessageType.Close);

        Console.WriteLine ("Done reading AV data");

        await avPipe.Writer.CompleteAsync();
        bool transcriptionSuccess = await transcriptionTask;
        Console.WriteLine ("Transcription " + (transcriptionSuccess ? "success" : "failure"));
    }
}
