namespace BackendTests.SpeechmaticsMessages;

using System.Text;
using System.Text.Json;
using Backend.Data.SpeechmaticsMessages.EndOfTranscriptMessage;

public class EndOfTranscriptMessageTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    private static readonly string messageValue = "EndOfTranscript";

    // Valid message must be accepted
    [Test]
    [Order(1)]
    public void ValidMessage_DoesntThrow()
    {
        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + messageValue + @"""",
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[]
        {
            "{",
            inner.ToString(),
            "}",
        });

        Assert.DoesNotThrow(() =>
        {
            JsonSerializer.Deserialize<EndOfTranscriptMessage>(outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<EndOfTranscriptMessage>(outer.ToString(), jsonOptions));
    }

    // Wrong message type must be rejected
    [Test]
    [Order(2)]
    public void WrongMessage_Throws()
    {
        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + "Not" + messageValue + @"""",
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[]
        {
            "{",
            inner.ToString(),
            "}",
        });

        Assert.Throws<ArgumentException>(() =>
        {
            JsonSerializer.Deserialize<EndOfTranscriptMessage>(outer.ToString(), jsonOptions);
        });
    }

    // Valid message must report correct type
    [Test]
    public void ValidMessage_CorrectType()
    {
        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + messageValue + @"""",
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[]
        {
            "{",
            inner.ToString(),
            "}",
        });

        EndOfTranscriptMessage message = JsonSerializer.Deserialize<EndOfTranscriptMessage>(outer.ToString(), jsonOptions)!;

        Assert.That(message.message, Is.EqualTo(messageValue));
    }
}
