namespace Backend.Services;

using System.Text;
using Backend.Data;

/// <summary>
/// Class responsible for exporting speech bubbles to SRT format.
/// </summary>
public class SrtConverter : ISubtitleConverter
{
    private readonly Stream outputStream;
    private int counter = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="SrtConverter"/> class.
    /// </summary>
    /// <param name="outputStream">The output stream to write the SRT content to.</param>
    public SrtConverter(Stream outputStream)
    {
        this.outputStream = outputStream;
    }

    /// <summary>
    /// Exports the speech bubbles to SRT format and writes the content to the output stream.
    /// </summary>
    /// <param name="speechBubbles">The list of speech bubbles to export.</param>
    public void ConvertSpeechBubble(SpeechBubble speechBubbles)
    {
        writeToStream(convertToSrt(speechBubbles));
    }

    /// <summary>
    /// Converts a list of speech bubbles to the SRT format.
    /// </summary>
    /// <param name="speechBubble">The list of speech bubbles to be converted.</param>
    /// <returns>A string representing the list of speech bubbles in SRT format.</returns>
    private string convertToSrt(SpeechBubble speechBubble)
    {
        var srtBuilder = new StringBuilder();

        // Adding the sequence number
        srtBuilder.AppendLine((++counter).ToString());

        // Formatting and adding the time range
        srtBuilder.AppendLine($"{formatTimeSrt(speechBubble.StartTime)} --> {formatTimeSrt(speechBubble.EndTime)}");

        // Adding the speech bubble content
        for (int i = 0; i < speechBubble.SpeechBubbleContent.Count; ++i)
        {
            if (i > 0) srtBuilder.Append(' ');
            srtBuilder.Append(speechBubble.SpeechBubbleContent[i].Word);
        }

        // Add a newline for the text of the speech bubble
        srtBuilder.AppendLine();
        if (counter < speechBubble.SpeechBubbleContent.Count)
        {
            srtBuilder.AppendLine();
        }

        return srtBuilder.ToString();
    }

    /// <summary>
    /// Formats the time in SRT format (hh:mm:ss,mmm).
    /// </summary>
    /// <param name="time">The time value to format.</param>
    /// <returns>The formatted time string.</returns>
    private static string formatTimeSrt(double time)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(time * 1000);
        return timeSpan.ToString(@"hh\:mm\:ss\,fff");
    }

    /// <summary>
    /// Writes the content to the output stream.
    /// </summary>
    /// <param name="content">The content to write.</param>
    private async void writeToStream(string content)
    {
        using (StreamWriter outputStreamWriter = new StreamWriter(
            stream: outputStream,
            encoding: new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: counter == 1),
            bufferSize: 4096,
            leaveOpen: true))
        {
            await outputStreamWriter.WriteAsync(content);
        }
    }
}
