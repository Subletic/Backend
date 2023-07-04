using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Backend.Data;

namespace Backend.Services;


/// <summary>
/// Class responsible for exporting speech bubbles to WebVTT format.
/// </summary>
public class WebVttExporter 
{
    private readonly Stream _outputStream;

    /// <summary>
    /// Initializes a new instance of the WebVttExporter class with the specified output stream.
    /// </summary>
    /// <param name="outputStream">The output stream to write the WebVTT content to.</param>
    public WebVttExporter(Stream outputStream)
    {
        _outputStream = outputStream;
    }

    /// <summary>
    /// Exports the speech bubbles to WebVTT format and writes the content to the output stream.
    /// </summary>
    /// <param name="speechBubbles">The list of speech bubbles to export.</param>
    public void ExportSpeechBubble(SpeechBubble speechBubble)
    {
        string webVttContent = ConvertToWebVttFormat(speechBubble);
        WriteToStream(webVttContent);
    }

    /// <summary>
    /// Converts the speech bubbles to WebVTT format.
    /// </summary>
    /// <param name="speechBubbles">The list of speech bubbles to convert.</param>
    /// <returns>The WebVTT-formatted content.</returns>
    public string ConvertToWebVttFormat(SpeechBubble speechBubble)
    {
        StringBuilder webVttBuilder = new StringBuilder();
        webVttBuilder.Append("WEBVTT");

        string startTime = FormatTime(speechBubble.StartTime);
        string endTime = FormatTime(speechBubble.EndTime);
        webVttBuilder.AppendLine();
        webVttBuilder.AppendLine();
        webVttBuilder.Append($"{startTime} --> {endTime}");

        foreach (var wordToken in speechBubble.SpeechBubbleContent)
        {
            webVttBuilder.AppendLine();
            webVttBuilder.Append(wordToken.Word);
        }

        return webVttBuilder.ToString();
    }


    /// <summary>
    /// Formats the time in WebVTT format (hh:mm:ss.mmm).
    /// </summary>
    /// <param name="time">The time value to format.</param>
    /// <returns>The formatted time string.</returns>
    static private string FormatTime(double time)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(time * 1000);
        return timeSpan.ToString(@"hh\:mm\:ss\.fff");
    }

    /// <summary>
    /// Writes the content to the output stream.
    /// </summary>
    /// <param name="content">The content to write.</param>
    public void WriteToStream(string content)
    {
        using (var writer = new StreamWriter(_outputStream, Encoding.UTF8, 4096, true))
        {
            writer.Write(content);
        }
    }
}
