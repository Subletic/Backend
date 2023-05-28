using Backend.Controllers;
using Backend.Data;

namespace BackendTests;

public class SpeechBubbleControllerTests
{
    [Test]
    public void SpeechBubbleController_Insert20NewWords_FirstSpeechBubbleAvailable()
    {
        var controller = new SpeechBubbleController();

        var testWord = new WordToken(word: "Test", confidence: 0.9f, timeStamp: 1, speaker: 1);
        for (var i = 0; i < 20; i++)
        {
            controller.HandleNewWord(testWord);
        }

        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(1));
    }

    [Test]
    public void SpeechBubbleController_Insert19NewWords_SpeechBubbleListEmpty()
    {
        var controller = new SpeechBubbleController();

        var testWord = new WordToken(word: "Test", confidence: 0.9f, timeStamp: 1, speaker: 1);
        for (var i = 0; i < 19; i++)
        {
            controller.HandleNewWord(testWord);
        }

        Assert.That(controller.GetSpeechBubbles(), Has.Count.EqualTo(0));
    }
}