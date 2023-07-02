using System;
using System.Collections.Generic;
using System.IO;
using Backend.Data;
using Backend.Services;
using NUnit.Framework;

namespace BackendTests
{
    [TestFixture]
    public class WebVttExporterTests
    {
        [Test]
        public void ExportSpeechBubble_WritesCorrectWebVttContentToStream()
        {
            var outputStream = new MemoryStream();
            var exporter = new WebVttExporter(outputStream);
            var speechBubble = new SpeechBubble(1, 0, 1.0, 2.5, new List<WordToken>
            {
                new WordToken("Hello", 0.9f, 1.0, 1.2, 1),
                new WordToken("world", 0.9f, 1.3, 1.5, 1),
                new WordToken("!", 0.9f, 1.6, 1.8, 1)
            });

            exporter.ExportSpeechBubble(speechBubble);
            outputStream.Position = 0;
            var reader = new StreamReader(outputStream);
            var exportedContent = reader.ReadToEnd();

            var expectedContent =
                @"WEBVTT

00:00:01.000 --> 00:00:02.500
Hello
world
!";
            Assert.AreEqual(expectedContent, exportedContent);
        }

        [TestCase("Hello", "world!")]
        [TestCase("", "")]
        public void ConvertToWebVttFormat_ReturnsCorrectWebVttContent(string word1, string word2)
        {
            var exporter = new WebVttExporter(null!);
            var speechBubble = new SpeechBubble(1, 0, 0.0, 1.0, new List<WordToken>
            {
                new WordToken(word1, 0.9f, 0.0, 0.5, 1),
                new WordToken(word2, 0.9f, 0.5, 1.0, 1)
            });

            var webVttContent = exporter.ConvertToWebVttFormat(speechBubble);

            Assert.IsFalse(string.IsNullOrEmpty(webVttContent));
        }

        [Test]
        public void WriteToStream_WritesContentToOutputStream()
        {
            var outputStream = new MemoryStream();
            var exporter = new WebVttExporter(outputStream);
            var content = "Test content";

            exporter.WriteToStream(content);
            outputStream.Position = 0;
            var reader = new StreamReader(outputStream);
            var writtenContent = reader.ReadToEnd();

            Assert.AreEqual(content, writtenContent);
        }

        [Test]
        public void ExportSpeechBubble_WithMultipleLines_WritesCorrectWebVttContentToStream()
        {
            var outputStream = new MemoryStream();
            var exporter = new WebVttExporter(outputStream);
            var speechBubble = new SpeechBubble(1, 0, 1.0, 3.5, new List<WordToken>
            {
                new WordToken("This", 0.9f, 1.0, 1.2, 1),
                new WordToken("is", 0.9f, 1.3, 1.5, 1),
                new WordToken("a", 0.9f, 1.6, 1.8, 1),
                new WordToken("multiline", 0.9f, 2.0, 2.5, 2),
                new WordToken("speech", 0.9f, 2.6, 2.8, 2),
                new WordToken("bubble", 0.9f, 2.9, 3.2, 2),
                new WordToken("!", 0.9f, 3.3, 3.5, 2)
            });

            exporter.ExportSpeechBubble(speechBubble);
            outputStream.Position = 0;
            var reader = new StreamReader(outputStream);
            var exportedContent = reader.ReadToEnd();

            var expectedContent =
                @"WEBVTT

00:00:01.000 --> 00:00:03.500
This
is
a
multiline
speech
bubble
!";
            Assert.AreEqual(expectedContent, exportedContent);
        }
    }
}

