namespace Backend.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Backend.Data;

    /// <summary>
    /// Class responsible for exporting speech bubbles to SRT format.
    /// </summary>
    public class SrtConverter : ISrtConverter
    {
        private readonly Stream outputStream;

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
        public void ConvertSpeechBubblesToSrt(List<SpeechBubble> speechBubbles)
        {
            writeToStream(convertToSrt(speechBubbles));
        }

        /// <summary>
        /// Converts a list of speech bubbles to the SRT format.
        /// </summary>
        /// <param name="speechBubbles">The list of speech bubbles to be converted.</param>
        /// <returns>A string representing the list of speech bubbles in SRT format.</returns>
        private string convertToSrt(List<SpeechBubble> speechBubbles)
        {
            var srtBuilder = new StringBuilder();
            int counter = 1; // Counter for the subtitle sequence numbers

            foreach (var bubble in speechBubbles)
            {
                // Adding the sequence number
                srtBuilder.AppendLine(counter.ToString());

                // Formatting and adding the time range
                srtBuilder.AppendLine($"{formatTimeSrt(bubble.StartTime)} --> {formatTimeSrt(bubble.EndTime)}");

                // Adding the text content
                var words = bubble.SpeechBubbleContent.Select(token => token.Word);
                srtBuilder.AppendLine(string.Join(" ", words));

                // Check if it's not the last bubble to add an extra newline
                if (counter < speechBubbles.Count)
                {
                    srtBuilder.AppendLine();
                }

                counter++;
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
                encoding: Encoding.UTF8,
                bufferSize: 4096,
                leaveOpen: true))
            {
                await outputStreamWriter.WriteAsync(content);
            }
        }
    }
}
