using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Backend.Data;

namespace Backend.Services
{

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
        public void ExportSpeechBubbles(List<SpeechBubble> speechBubbles)
        {
            string webVttContent = ConvertToWebVttFormat(speechBubbles);
            WriteToStream(webVttContent);
        }

        /// <summary>
        /// Converts the speech bubbles to WebVTT format.
        /// </summary>
        /// <param name="speechBubbles">The list of speech bubbles to convert.</param>
        /// <returns>The WebVTT-formatted content.</returns>
        private string ConvertToWebVttFormat(List<SpeechBubble> speechBubbles)
        {
            // TODO: Implement the conversion logic
        }

        /// <summary>
        /// Formats the time in WebVTT format (hh:mm:ss.mmm).
        /// </summary>
        /// <param name="time">The time value to format.</param>
        /// <returns>The formatted time string.</returns>
        private string FormatTime(double time)
        {
            int hours = (int)(time / 3600);
            int minutes = (int)((time % 3600) / 60);
            int seconds = (int)(time % 60);
            int milliseconds = (int)((time - Math.Truncate(time)) * 1000);

            return $"{hours:D2}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
        }

        /// <summary>
        /// Writes the content to the output stream.
        /// </summary>
        /// <param name="content">The content to write.</param>
        private void WriteToStream(string content)
        {
            using (var writer = new StreamWriter(_outputStream, Encoding.UTF8, 4096, true))
            {
                writer.Write(content);
            }
        }
    }
}
