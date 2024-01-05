#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using Backend.Data;

/// <summary>
/// Class responsible for exporting speech bubbles to WebVTT format.
/// </summary>
public class WebVttConverter : ISubtitleConverter
{
    private readonly Stream outputStream;

    /// <summary>
    /// Initializes a new instance of the WebVttConverter class with the specified output stream.
    /// </summary>
    /// <param name="outputStream">The output stream to write the WebVTT content to.</param>
    public WebVttConverter(Stream outputStream)
    {
        this.outputStream = outputStream;

        // header
        WriteToStream("WEBVTT");
    }

    /// <summary>
    /// Exports the speech bubbles to WebVTT format and writes the content to the output stream.
    /// </summary>
    /// <param name="speechBubble">The list of speech bubbles to export.</param>
    public void ConvertSpeechBubble(SpeechBubble speechBubble)
    {
        WriteToStream(convertToWebVttFormat(speechBubble));
    }

    /// <summary>
    /// Converts the speech bubbles to WebVTT format.
    /// </summary>
    /// <param name="speechBubble">The list of speech bubbles to convert.</param>
    /// <returns>The WebVTT-formatted content.</returns>
    private static string convertToWebVttFormat(SpeechBubble speechBubble)
    {
        StringBuilder webVttBuilder = new StringBuilder();

        string startTime = FormatTime(speechBubble.StartTime);
        string endTime = FormatTime(speechBubble.EndTime);

        webVttBuilder.AppendLine();
        webVttBuilder.AppendLine();
        webVttBuilder.Append($"{startTime} --> {endTime}");

        if (speechBubble.SpeechBubbleContent.Count > 0)
        {
            webVttBuilder.AppendLine();
            webVttBuilder.Append(speechBubble.SpeechBubbleContent[0].Word);
        }

        for (int i = 1; i < speechBubble.SpeechBubbleContent.Count; ++i)
        {
            webVttBuilder.Append(' ');
            webVttBuilder.Append(speechBubble.SpeechBubbleContent[i].Word);
        }

        return webVttBuilder.ToString();
    }

    /// <summary>
    /// Formats the time in WebVTT format (hh:mm:ss.mmm).
    /// </summary>
    /// <param name="time">The time value to format.</param>
    /// <returns>The formatted time string.</returns>
    private static string FormatTime(double time)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(time * 1000);
        return timeSpan.ToString(@"hh\:mm\:ss\.fff");
    }

    /// <summary>
    /// Writes the content to the output stream.
    /// </summary>
    /// <param name="content">The content to write.</param>
    private async void WriteToStream(string content)
    {
        using (StreamWriter outputStreamWriter = new StreamWriter(
            stream: outputStream, // Der Ziel-Stream, in den geschrieben wird
            encoding: Encoding.UTF8, // Die Zeichencodierung (hier: UTF-8)
            bufferSize: 4096, // Die Puffergröße für optimale Leistung
            leaveOpen: true)) // Gibt an, ob der Stream geöffnet bleiben soll
            await outputStreamWriter.WriteAsync(content);
    }
}
