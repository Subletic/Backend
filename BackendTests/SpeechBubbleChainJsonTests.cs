namespace BackendTests;

using Backend.Data;

public class SpeechBubbleChainJsonTests
{
    private readonly List<WordToken> testWordList;

    public SpeechBubbleChainJsonTests()
    {
        testWordList = new List<WordToken>
        {
            new("Hello", 1, 1, 10, 4),
            new("World", 1, 11, 12, 4),
            new("!", 1, 12, 12.5, 4),
        };
    }

    [Test]
    public void ToSpeechBubbleList_InsertValidSpeechBubbleChainJson_MappedToCorrectSpeechBubble()
    {
        // Arrange
        var speechBubbleJson = new SpeechBubbleJson(1, 4, 1, 12, testWordList);
        var speechBubbleChainJson = new SpeechBubbleChainJson(new List<SpeechBubbleJson> { speechBubbleJson });
        var expectedResult = new List<SpeechBubble> { new SpeechBubble(1, 4, 1, 12, testWordList) };

        // Act
        var resultingSpeechBubbleList = speechBubbleChainJson.ToSpeechBubbleList();
        var resultingWordList = resultingSpeechBubbleList.First().SpeechBubbleContent;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(resultingSpeechBubbleList.First().Id, Is.EqualTo(expectedResult.First().Id));
            Assert.That(resultingSpeechBubbleList.First().Speaker, Is.EqualTo(expectedResult.First().Speaker));
            Assert.That(resultingSpeechBubbleList.First().StartTime, Is.EqualTo(expectedResult.First().StartTime));
            Assert.That(resultingSpeechBubbleList.First().EndTime, Is.EqualTo(expectedResult.First().EndTime));

            for (var i = 0; i < resultingWordList.Count; i++)
            {
                Assert.That(resultingWordList[i].Word, Is.EqualTo(testWordList[i].Word));
                Assert.That(resultingWordList[i].StartTime, Is.EqualTo(testWordList[i].StartTime));
                Assert.That(resultingWordList[i].EndTime, Is.EqualTo(testWordList[i].EndTime));
                Assert.That(resultingWordList[i].Speaker, Is.EqualTo(testWordList[i].Speaker));
                Assert.That(resultingWordList[i].Confidence, Is.EqualTo(testWordList[i].Confidence));
            }
        });
    }
}
