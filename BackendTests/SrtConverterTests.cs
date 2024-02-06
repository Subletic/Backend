namespace BackendTests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Backend.ClientCommunication;
    using Backend.Data;
    using NUnit.Framework;

    [TestFixture]
    public class SrtConverterTests
    {
        private static IEnumerable<object[]> exportDataForSrt()
        {
            var testData = new[]
            {
                // 1 Bubble, no Tokens
                new object[]
                {
                    new List<SpeechBubble>
                    {
                        new SpeechBubble(1, 1, 0.0, 1.0, new List<WordToken> { }),
                    },
                    string.Join(Environment.NewLine, new string[]
                    {
                        "1",
                        "00:00:00,000 --> 00:00:01,000",
                        "",
                        "",
                    }),
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
                    string.Join(Environment.NewLine, new string[]
                    {
                        "1",
                        "00:00:00,200 --> 00:00:00,800",
                        "Test",
                        "",
                    }),
                },

                // Multiple Bubbles, multiple Tokens, 1 Speaker
                new object[]
                {
                    new List<SpeechBubble>
                    {
                        new SpeechBubble(1, 1, 1.0, 2.0, new List<WordToken>
                        {
                            new WordToken("Hello", 0.9f, 1.0, 1.5, 1),
                            new WordToken("world", 0.9f, 1.5, 2.0, 1),
                        }),
                        new SpeechBubble(2, 1, 3.0, 4.0, new List<WordToken>
                        {
                            new WordToken("Goodbye", 0.9f, 3.0, 3.5, 1),
                            new WordToken("world", 0.9f, 3.5, 4.0, 1),
                        }),
                    },
                    string.Join(Environment.NewLine, new string[]
                    {
                        "1",
                        "00:00:01,000 --> 00:00:02,000",
                        "Hello world",
                        "",
                        "2",
                        "00:00:03,000 --> 00:00:04,000",
                        "Goodbye world",
                        "",
                    }),
                },
            };

            foreach (object[] test in testData)
            yield return test;
    }

        [Test]
        [TestCaseSource(nameof(exportDataForSrt))]
        public void ConvertSpeechBubbles_ToSrtFormat_HandlesExampleCorrectly(List<SpeechBubble> speechBubbles, string expectedContent)
        {
            performTestSrt(speechBubbles, expectedContent);
        }

        private void performTestSrt(List<SpeechBubble> speechBubbles, string expectedContent)
        {
            // Initialize SRT converter
            Stream outputStream = new MemoryStream();
            SrtConverter srtConverter = new SrtConverter(outputStream);

            foreach (var speechBubble in speechBubbles)
            {
                srtConverter.ConvertSpeechBubble(speechBubble);
            }

            // Read converter result
            outputStream.Position = 0;
            string exportedContent = "";
            using (var reader = new StreamReader(outputStream))
                exportedContent = reader.ReadToEnd();

            // Verify if the exported output matches the expected output
            Assert.That(exportedContent, Is.EqualTo(expectedContent));
        }
    }
}
