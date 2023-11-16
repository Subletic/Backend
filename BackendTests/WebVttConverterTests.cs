namespace BackendTests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Backend.Data;
    using Backend.Services;
    using NUnit.Framework;

    [TestFixture]
    public class WebVttConverterTests
    {
        private static IEnumerable<object[]> exportData()
        {
            var testData = new[]
            {
                // No Bubbles
                new object[]
                {
                    new List<SpeechBubble> { },
                    @"WEBVTT",
                },

                // 1 Bubble, no Tokens
                new object[]
                {
                    new List<SpeechBubble>
                    {
                        new SpeechBubble(1, 1, 0.0, 1.0, new List<WordToken> { }),
                    },
                    @"WEBVTT

00:00:00.000 --> 00:00:01.000",
                },

                // 1 Bubble, 1 Token, 1 Speaker
                new object[]
                {
                    new List<SpeechBubble>
                    {
                        new SpeechBubble(1, 1, 0.2, 0.8, new List<WordToken>
                        {
                             new WordToken("Test", 0.5f, 0.2, 0.8, 1),
                        }),
                    },
                    @"WEBVTT

00:00:00.200 --> 00:00:00.800
Test",
                },

                // 1 Bubble, multiple Tokens, 1 Speaker
                new object[]
                {
                    new List<SpeechBubble>
                    {
                        new SpeechBubble(1, 1, 1.0, 2.5, new List<WordToken>
                        {
                            new WordToken("Hello", 0.9f, 1.0, 1.2, 1),
                            new WordToken("world!", 0.9f, 1.3, 1.5, 1),
                        }),
                    },
                    @"WEBVTT

00:00:01.000 --> 00:00:02.500
Hello world!",
                },

                // Multiple Bubbles, multiple Tokens, 1 Speaker
                new object[]
                {
                    new List<SpeechBubble>
                    {
                        new SpeechBubble(1, 1, 7.8, 9.6, new List<WordToken>
                        {
                            new WordToken("Hello,", 0.9f, 7.8, 8.35, 1),
                            new WordToken("everyone!", 0.4f, 8.4, 9.6, 1),
                        }),
                        new SpeechBubble(2, 1, 11.0, 12.5, new List<WordToken>
                        {
                            new WordToken("How", 1.0f, 11.0, 11.45, 1),
                            new WordToken("are", 0.9f, 11.5, 11.95, 1),
                            new WordToken("you?", 1.0f, 12.0, 12.5, 1),
                        }),
                    },
                    @"WEBVTT

00:00:07.800 --> 00:00:09.600
Hello, everyone!

00:00:11.000 --> 00:00:12.500
How are you?",
                },
            };

            foreach (object[] test in testData)
                yield return test;
        }

        [Test]
        [TestCaseSource(nameof(exportData))]
        public void ConvertSpeechBubble_HandlesExampleCorrectly(List<SpeechBubble> speechBubbles, string expectedContent)
        {
            // Init converter
            Stream outputStream = new MemoryStream();
            ISubtitleConverter converter = new WebVttConverter(outputStream);

            // Push SpeechBubbles through the converter
            foreach (SpeechBubble speechBubble in speechBubbles)
                converter.ConvertSpeechBubble(speechBubble);

            // Read converter result
            outputStream.Position = 0;
            string exportedContent = "";
            using (var reader = new StreamReader(outputStream))
                exportedContent = reader.ReadToEnd();

            // Überprüfen Sie, ob die exportierte Ausgabe der erwarteten Ausgabe entspricht
            Assert.That(exportedContent, Is.EqualTo(expectedContent));
        }
    }
}
