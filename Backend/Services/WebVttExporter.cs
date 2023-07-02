using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Backend.Data;

namespace Backend.Services
{
    public class WebVttExporter
    {
        private readonly Stream _outputStream;

        public WebVttExporter(Stream outputStream)
        {
            _outputStream = outputStream;
        }

        public void ExportSpeechBubble(SpeechBubble speechBubble)
        {
            string webVttContent = ConvertToWebVttFormat(speechBubble);
            WriteToStream(webVttContent);
        }

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

        private string FormatTime(double time)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(time * 1000);
            return timeSpan.ToString(@"hh\:mm\:ss\.fff");
        }

        public void WriteToStream(string content)
        {
            using (var writer = new StreamWriter(_outputStream, Encoding.UTF8, 4096, true))
            {
                writer.Write(content);
            }
        }
    }
}
