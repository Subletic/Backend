using Backend.Data;
using Backend.Services;

namespace BackendTests
{
    [TestFixture]
    public class SpeechBubbleListServiceTests
    {
        private SpeechBubbleListService speechBubbleListService;
        private readonly SpeechBubble testSpeechBubble1;
        private readonly SpeechBubble testSpeechBubble2;

        public SpeechBubbleListServiceTests()
        {
            speechBubbleListService = new SpeechBubbleListService();

            var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1, endTime: 2, speaker: 1);
            var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 3, endTime: 4, speaker: 2);
            var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 10, endTime: 11, speaker: 2);
            var fourthWord = new WordToken(word: "Test4", confidence: 0.7f, startTime: 12, endTime: 14, speaker: 2);

            testSpeechBubble1 = new SpeechBubble
            (
                1,
                1,
                1,
                1,
                new List<WordToken> { firstWord, secondWord, thirdWord, fourthWord }
            );

            testSpeechBubble2 = new SpeechBubble
            (
                2,
                1,
                1,
                1,
                new List<WordToken> { firstWord, secondWord, thirdWord, fourthWord }
            );
        }

        [SetUp]
        public void SetUp()
        {
            speechBubbleListService = new SpeechBubbleListService();
        }

        [Test]
        public void GetSpeechBubbles_ReturnsEmptyList_WhenNoSpeechBubblesAdded()
        {
            var speechBubbles = speechBubbleListService.GetSpeechBubbles();

            Assert.That(speechBubbles, Is.Empty);
        }

        [Test]
        public void AddNewSpeechBubble_AddsSpeechBubbleToList()
        {
            speechBubbleListService.AddNewSpeechBubble(testSpeechBubble1);
            var speechBubbles = speechBubbleListService.GetSpeechBubbles();

            Assert.That(speechBubbles, Has.Count.EqualTo(1));
            Assert.That(speechBubbles.First!.Value, Is.EqualTo(testSpeechBubble1));
        }

        [Test]
        public void DeleteOldestSpeechBubble_RemovesOldestSpeechBubbleFromList()
        {
            speechBubbleListService.AddNewSpeechBubble(testSpeechBubble1);
            speechBubbleListService.AddNewSpeechBubble(testSpeechBubble2);

            speechBubbleListService.DeleteOldestSpeechBubble();
            var speechBubbles = speechBubbleListService.GetSpeechBubbles();

            Assert.That(speechBubbles, Has.Count.EqualTo(1));
            Assert.That(speechBubbles.First!.Value, Is.EqualTo(testSpeechBubble2));
        }

        [Test]
        public void ReplaceSpeechBubble_ReplacesSpeechBubbleWithSameIdLowSpeechBubbleCount()
        {
            var fourthWord = new WordToken(word: "Test4", confidence: 0.7f, startTime: 12, endTime: 14, speaker: 2);

            var testSpeechBubble1 = new SpeechBubble
            (
                1,
                1,
                1,
                1,
                new List<WordToken> { fourthWord }
            );

            var testSpeechBubble2 = new SpeechBubble
            (
                2,
                1,
                1,
                1,
                new List<WordToken> { fourthWord }
            );

            var testSpeechBubble3 = new SpeechBubble
            (
                1,
                1,
                1,
                1,
                new List<WordToken> { fourthWord }
            );

            speechBubbleListService.AddNewSpeechBubble(testSpeechBubble1);
            speechBubbleListService.AddNewSpeechBubble(testSpeechBubble2);

            speechBubbleListService.ReplaceSpeechBubble(testSpeechBubble3);
            var speechBubbles = speechBubbleListService.GetSpeechBubbles();

            Assert.That(speechBubbles, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(speechBubbles.First!.Value, Is.EqualTo(testSpeechBubble3));
                Assert.That(speechBubbles.Last!.Value, Is.EqualTo(testSpeechBubble2));
            });
        }

        [Test]
        public void ReplaceSpeechBubble_ReplacesSpeechBubbleWithSameIdHighSpeechBubbleCount()
        {
            var fourthWord = new WordToken(word: "Test4", confidence: 0.7f, startTime: 12, endTime: 14, speaker: 2);

            var testSpeechBubble1 = new SpeechBubble
            (
                1,
                1,
                1,
                1,
                new List<WordToken> { fourthWord }
            );

            var testSpeechBubble2 = new SpeechBubble
            (
                2,
                1,
                1,
                1,
                new List<WordToken> { fourthWord }
            );

            var testSpeechBubble3 = new SpeechBubble
            (
                3,
                1,
                1,
                1,
                new List<WordToken> { fourthWord }
            );

            var testSpeechBubble4 = new SpeechBubble
            (
                2,
                1,
                1,
                1,
                new List<WordToken> { fourthWord }
            );

            speechBubbleListService.AddNewSpeechBubble(testSpeechBubble1);
            speechBubbleListService.AddNewSpeechBubble(testSpeechBubble2);
            speechBubbleListService.AddNewSpeechBubble(testSpeechBubble3);

            speechBubbleListService.ReplaceSpeechBubble(testSpeechBubble4);
            var speechBubbles = speechBubbleListService.GetSpeechBubbles();

            Assert.That(speechBubbles, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(speechBubbles.First!.Value, Is.EqualTo(testSpeechBubble1));
                Assert.That(speechBubbles.ElementAt(1), Is.EqualTo(testSpeechBubble4));
            });
        }

        [Test]
        public void ReplaceSpeechBubble_ReplaceIntoEmptyListAddsElementToList()
        {
            speechBubbleListService.ReplaceSpeechBubble(testSpeechBubble1);

            var speechBubbles = speechBubbleListService.GetSpeechBubbles();
            Assert.That(speechBubbles, Has.Count.EqualTo(1));
        }

        [Test]
        public void ReplaceSpeechBubble_CreationTimeDoesntChange()
        {
            var creationTime = testSpeechBubble1.CreationTime;
            var newSpeechBubble = new SpeechBubble(
                testSpeechBubble1.Id, testSpeechBubble1.Speaker, testSpeechBubble1.StartTime,
                testSpeechBubble1.EndTime, testSpeechBubble1.SpeechBubbleContent);

            newSpeechBubble.CreationTime += TimeSpan.FromMinutes(2);

            speechBubbleListService.ReplaceSpeechBubble(testSpeechBubble1);
            speechBubbleListService.ReplaceSpeechBubble(newSpeechBubble);

            var speechBubbles = speechBubbleListService.GetSpeechBubbles();

            Assert.That(speechBubbles.First!.Value.CreationTime, Is.EqualTo(creationTime));
        }
    }
}
