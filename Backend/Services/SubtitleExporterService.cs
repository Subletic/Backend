﻿using Backend.Data;
using Backend.Services;

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Services;

/**
  *  <summary>
  *  Service responsible for exporting finished subtitles back over the WebSocket connection.
  *  </summary>
  */
public class SubtitleExporterService : ISubtitleExporterService
{
    /**
      *  <summary>
      *  Maximum amount of data to read from the converted subtitle at once, in bytes
      *  </summary>
      */
    private const int MAXIMUM_READ_SIZE = 4096;

    /**
      *  <summary>
      *  Pipe for reading converted subtitles from the converter
      *  </summary>
      */
    private Pipe subtitlePipe;

    /**
      *  <summary>
      *  The converter for translating SpeechBubbles into a preferred subtitle format
      *  </summary>
      */
    private ISubtitleConverter subtitleConverter;

    /**
      *  <summary>
      *  Basic constructor.
      *  </summary>
      */
    public SubtitleExporterService()
    {
        subtitlePipe = new Pipe();
        subtitleConverter = new WebVttConverter (subtitlePipe.Writer.AsStream (leaveOpen: true));
    }

    /**
      *  <summary>
      *  Starts up the sending side of the processing pipeline.
      *  Awaits subtitles to be pushed into <c>ExportSubtitle</c>, receives converted subtitles from
      *  chosen subtitle converter and pushes them back through the WebSocket connection.
      *  </summary>
      */
    public async Task Start(WebSocket webSocket, CancellationTokenSource ctSource)
    {
        Stream subtitleReaderStream = subtitlePipe.Reader.AsStream (leaveOpen: false);
        byte[] buffer = new byte[MAXIMUM_READ_SIZE];

        Console.WriteLine ("Start sending subtitles over WebSocket");

        try
        {
            while (true)
            {
                Console.WriteLine ("Trying to read subtitles");
                int readCount = 0;
                try
                {
                    // "block" here until at least 1 byte can be read
                    readCount = await subtitleReaderStream.ReadAtLeastAsync (buffer, 1, true, ctSource.Token);
                }
                catch (EndOfStreamException)
                {
                    Console.WriteLine ("End of stream reached");
                    break;
                }

                Console.WriteLine ("Have subtitles ready to send");

                await webSocket.SendAsync (new ReadOnlyMemory<byte> (buffer, 0, readCount),
                    WebSocketMessageType.Text,
                    false,
                    ctSource.Token);
                Console.WriteLine ("Subtitles sent");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine ("Subtitle export has been cancelled");
        }

        Console.WriteLine ("Done sending subtitles over WebSocket");
    }

    /**
      *  <summary>
      *  Push a finished <c>SpeechBubble</c> through the chosen converter
      *  </summary>
      */
    public Task ExportSubtitle(SpeechBubble speechBubble)
    {
        subtitleConverter.ConvertSpeechBubble(speechBubble);
        return Task.CompletedTask;
    }
}