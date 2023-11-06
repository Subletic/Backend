using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Services;
using NUnit.Framework;

namespace BackendTests
{
    [TestFixture]
    public class WebVttConverterTests
    {
        [Test]
        public void ConvertSpeechBubble_WritesCorrectWebVttContentToStream()
        {
            // Erstellen Sie einen MemoryStream, um die Ausgabe zu erfassen
            var outputStream = new MemoryStream();
            var converter = new WebVttConverter(outputStream);

            // Erstellen Sie eine Test-SpeechBubble
            var speechBubble = new SpeechBubble(1, 0, 1.0, 2.5, new List<WordToken>
            {
                new WordToken("Hello", 0.9f, 1.0, 1.2, 1),
                new WordToken("world", 0.9f, 1.3, 1.5, 1),
                new WordToken("!", 0.9f, 1.6, 1.8, 1)
            });

            // Rufen Sie die Methode ConvertSpeechBubble auf
            converter.ConvertSpeechBubble(speechBubble);

            // Stellen Sie den MemoryStream auf den Anfang zurück
            outputStream.Position = 0;
            var reader = new StreamReader(outputStream);
            var exportedContent = reader.ReadToEnd();

            // Erwartete WebVTT-Ausgabe
            var expectedContent =
                @"WEBVTT

00:00:01.000 --> 00:00:02.500
Hello
world
!";

            // Überprüfen Sie, ob die exportierte Ausgabe der erwarteten Ausgabe entspricht
            Assert.That(exportedContent, Is.EqualTo(expectedContent));
        }
    }
}
