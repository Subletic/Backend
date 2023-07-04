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
            Assert.That(exportedContent, Is.EqualTo(expectedContent));
        }

        [TestCase("Hello", "world!")]
        [TestCase("", "")]
        public void ConvertToWebVttFormat_ReturnsCorrectWebVttContent(string word1, string word2)
        {
            var exporter = new WebVttExporter(new MemoryStream());
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

            // required header is written on construction
            Assert.That(writtenContent, Is.EqualTo ("WEBVTT" + content));
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
            Assert.That(exportedContent, Is.EqualTo(expectedContent));
        }

        [Test]
        public void ExportSpeechBubble_WithMultipleBubbles_WritesCorrectWebVttContentToStream()
        {
            var outputStream = new MemoryStream();
            var exporter = new WebVttExporter(outputStream);
            int speakerId = 0;
            var speechBubbles = new SpeechBubble[] {
                new SpeechBubble(1, speakerId, 0.5d, 2.5d, new List<WordToken> {
                    new WordToken("This", 1.0f, 0.5d, 0.75d, speakerId),
                    new WordToken("is", 1.0f, 0.75d, 0.95d, speakerId),
                    new WordToken("the", 1.0f, 0.95d, 1.15d, speakerId),
                    new WordToken("first", 1.0f, 1.15d, 2.0d, speakerId),
                    new WordToken("speech", 1.0f, 2.0d, 2.25d, speakerId),
                    new WordToken("bubble", 1.0f, 2.25d, 2.5d, speakerId),
                    new WordToken("?", 1.0f, 2.5d, 2.5d, speakerId),
                }),
                new SpeechBubble(1, speakerId, 3.5d, 5.15d, new List<WordToken> {
                    new WordToken("And", 1.0f, 3.5d, 3.75d, speakerId),
                    new WordToken("this", 1.0f, 3.75d, 4.25d, speakerId),
                    new WordToken("is", 1.0f, 4.25d, 4.45d, speakerId),
                    new WordToken("the", 1.0f, 4.45d, 4.65d, speakerId),
                    new WordToken("second", 1.0f, 4.65d, 4.95d, speakerId),
                    new WordToken("one", 1.0f, 4.95d, 5.15d, speakerId),
                    new WordToken("!", 1.0f, 5.15d, 5.15d, speakerId),
                })
            };

            foreach (var speechBubble in speechBubbles)
            {
                exporter.ExportSpeechBubble(speechBubble);
            }

            outputStream.Position = 0;
            var reader = new StreamReader(outputStream);
            var exportedContent = reader.ReadToEnd();

            var expectedContent =
                @"WEBVTT

00:00:00.500 --> 00:00:02.500
This
is
the
first
speech
bubble
?

00:00:03.500 --> 00:00:05.150
And
this
is
the
second
one
!";
            Assert.That(exportedContent, Is.EqualTo(expectedContent));

        }
    }
}

