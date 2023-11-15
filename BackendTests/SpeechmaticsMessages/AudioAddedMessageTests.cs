namespace BackendTests.SpeechmaticsMessages;

using System.Text;
using System.Text.Json;
using Backend.Data.SpeechmaticsMessages.AudioAddedMessage;

public class AudioAddedMessageTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    private static readonly string messageValue = "AudioAdded";

    // Valid message must be accepted
    [Test]
    [Order(1)]
    public void ValidMessage_DoesntThrow()
    {
        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + messageValue + @"""",
            @"""seq_no"": 0",
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
            JsonSerializer.Deserialize<AudioAddedMessage>(outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<AudioAddedMessage>(outer.ToString(), jsonOptions));
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
            @"""seq_no"": 0",
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
            JsonSerializer.Deserialize<AudioAddedMessage>(outer.ToString(), jsonOptions);
        });
    }

    // Invalid seq_no type must be rejected
    [Test]
    [Order(3)]
    public void InvalidSeqno_Throws()
    {
        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + "Not" + messageValue + @"""",
            @"""seq_no"": -22",
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[]
        {
            "{",
            inner.ToString(),
            "}",
        });

        Assert.Throws<JsonException>(() =>
        {
            JsonSerializer.Deserialize<AudioAddedMessage>(outer.ToString(), jsonOptions);
        });
    }

    // Valid message must report correct contents
    [Test]
    public void ValidMessage_CorrectContent()
    {
        var seq_noValue = 12345;

        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + messageValue + @"""",
            @"""seq_no"": " + seq_noValue,
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[]
        {
            "{",
            inner.ToString(),
            "}",
        });

        AudioAddedMessage message = JsonSerializer.Deserialize<AudioAddedMessage>(outer.ToString(), jsonOptions)!;

        Assert.That(message.message, Is.EqualTo(messageValue));
        Assert.That(message.seq_no, Is.EqualTo(seq_noValue));
    }
}
