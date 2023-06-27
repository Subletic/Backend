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
            // Arrange
            var outputStream = new MemoryStream();
            var exporter = new WebVttExporter(outputStream);
            var speechBubble = new SpeechBubble(1, 0, 1.0, 2.5, new List<WordToken>
            {
                new WordToken("Hello", 0.9f, 1.0, 1.2, 1),
                new WordToken("world", 0.9f, 1.3, 1.5, 1),
                new WordToken("!", 0.9f, 1.6, 1.8, 1)
            });

            // Act
            exporter.ExportSpeechBubble(speechBubble);
            outputStream.Position = 0;
            var reader = new StreamReader(outputStream);
            var exportedContent = reader.ReadToEnd();

            // Assert
            var expectedContent =
                @"WEBVTT

00:00:01.000 --> 00:00:02.500
Hello
world
!";
            Assert.AreEqual(expectedContent, exportedContent);
        }
    }
}
