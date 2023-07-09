using Backend.Data.SpeechmaticsMessages;
using System.Text;
using System.Text.Json;

namespace BackendTests.SpeechmaticsMessages;

public class EndOfTranscriptMessageTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    private static readonly string messageValue = "EndOfTranscript";

    [Test, Order(1)]
    // Valid message must be accepted
    public void ValidMessage_DoesntThrow()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + messageValue + @"""",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.DoesNotThrow (() => {
            JsonSerializer.Deserialize<EndOfTranscriptMessage> (outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<EndOfTranscriptMessage> (outer.ToString(), jsonOptions));
    }

    [Test, Order(2)]
    // Wrong message type must be rejected
    public void WrongMessage_Throws()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + "Not" + messageValue + @"""",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.Throws<ArgumentException> (() => {
            JsonSerializer.Deserialize<EndOfTranscriptMessage> (outer.ToString(), jsonOptions);
        });
    }

    [Test]
    // Valid message must report correct type
    public void ValidMessage_CorrectType()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + messageValue + @"""",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        EndOfTranscriptMessage message = JsonSerializer.Deserialize<EndOfTranscriptMessage> (outer.ToString(),
            jsonOptions)!;

        Assert.That(message.message, Is.EqualTo (messageValue));
    }

}
