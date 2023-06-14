using Backend.Data;
using Backend.Services;

namespace BackendTests
{
    [TestFixture]
    public class SpeechBubbleListServiceTests
    {
        private SpeechBubbleListService _speechBubbleListService;
        private readonly SpeechBubble _testSpeechBubble1;
        private readonly SpeechBubble _testSpeechBubble2;

        public SpeechBubbleListServiceTests()
        {
            _speechBubbleListService = new SpeechBubbleListService();

            var firstWord = new WordToken(word: "Test", confidence: 0.9f, startTime: 1, endTime: 2, speaker: 1);
            var secondWord = new WordToken(word: "Test2", confidence: 0.7f, startTime: 3, endTime: 4, speaker: 2);
            var thirdWord = new WordToken(word: "Test3", confidence: 0.7f, startTime: 10, endTime: 11, speaker: 2);
            var fourthWord = new WordToken(word: "Test4", confidence: 0.7f, startTime: 12, endTime: 14, speaker: 2);

            _testSpeechBubble1 = new SpeechBubble
            (
                1, 
                1,
                1,
                1,
                new List<WordToken> { firstWord, secondWord, thirdWord, fourthWord }
            );

            _testSpeechBubble2 = new SpeechBubble
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
            _speechBubbleListService = new SpeechBubbleListService();
        }

        [Test]
        public void GetSpeechBubbles_ReturnsEmptyList_WhenNoSpeechBubblesAdded()
        {
            var speechBubbles = _speechBubbleListService.GetSpeechBubbles();

            Assert.That(speechBubbles, Is.Empty);
        }

        [Test]
        public void AddNewSpeechBubble_AddsSpeechBubbleToList()
        {
            _speechBubbleListService.AddNewSpeechBubble(_testSpeechBubble1);
            var speechBubbles = _speechBubbleListService.GetSpeechBubbles();

            Assert.That(speechBubbles, Has.Count.EqualTo(1));
            Assert.That(speechBubbles.First!.Value, Is.EqualTo(_testSpeechBubble1));
        }

        [Test]
        public void DeleteOldestSpeechBubble_RemovesOldestSpeechBubbleFromList()
        {
            _speechBubbleListService.AddNewSpeechBubble(_testSpeechBubble1);
            _speechBubbleListService.AddNewSpeechBubble(_testSpeechBubble2);

            _speechBubbleListService.DeleteOldestSpeechBubble();
            var speechBubbles = _speechBubbleListService.GetSpeechBubbles();

            Assert.That(speechBubbles, Has.Count.EqualTo(1));
            Assert.That(speechBubbles.First!.Value, Is.EqualTo(_testSpeechBubble2));
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

            _speechBubbleListService.AddNewSpeechBubble(testSpeechBubble1);
            _speechBubbleListService.AddNewSpeechBubble(testSpeechBubble2);

            _speechBubbleListService.ReplaceSpeechBubble(testSpeechBubble3);
            var speechBubbles = _speechBubbleListService.GetSpeechBubbles();

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

            _speechBubbleListService.AddNewSpeechBubble(testSpeechBubble1);
            _speechBubbleListService.AddNewSpeechBubble(testSpeechBubble2);
            _speechBubbleListService.AddNewSpeechBubble(testSpeechBubble3);

            _speechBubbleListService.ReplaceSpeechBubble(testSpeechBubble4);
            var speechBubbles = _speechBubbleListService.GetSpeechBubbles();

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
            _speechBubbleListService.ReplaceSpeechBubble(_testSpeechBubble1);

            var speechBubbles = _speechBubbleListService.GetSpeechBubbles();
            Assert.That(speechBubbles, Has.Count.EqualTo(1));
        }
    }
}