using Backend.Data.SpeechmaticsMessages.AudioAddedMessage;
using System.Text;
using System.Text.Json;

namespace BackendTests.SpeechmaticsMessages;

public class AudioAddedMessageTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    private static readonly string messageValue = "AudioAdded";

    [Test, Order(1)]
    // Valid message must be accepted
    public void ValidMessage_DoesntThrow()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + messageValue + @"""",
            @"""seq_no"": 0",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.DoesNotThrow (() => {
            JsonSerializer.Deserialize<AudioAddedMessage> (outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<AudioAddedMessage> (outer.ToString(), jsonOptions));
    }

    [Test, Order(2)]
    // Wrong message type must be rejected
    public void WrongMessage_Throws()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + "Not" + messageValue + @"""",
            @"""seq_no"": 0",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.Throws<ArgumentException> (() => {
            JsonSerializer.Deserialize<AudioAddedMessage> (outer.ToString(), jsonOptions);
        });
    }

    [Test, Order(3)]
    // Invalid seq_no type must be rejected
    public void InvalidSeqno_Throws()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + "Not" + messageValue + @"""",
            @"""seq_no"": -22",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.Throws<JsonException> (() => {
            JsonSerializer.Deserialize<AudioAddedMessage> (outer.ToString(), jsonOptions);
        });
    }


    [Test]
    // Valid message must report correct contents
    public void ValidMessage_CorrectContent()
    {
        var seq_noValue = 12345;

        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + messageValue + @"""",
            @"""seq_no"": " + seq_noValue,
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        AudioAddedMessage message = JsonSerializer.Deserialize<AudioAddedMessage> (outer.ToString(),
            jsonOptions)!;

        Assert.That(message.message, Is.EqualTo (messageValue));
        Assert.That(message.seq_no, Is.EqualTo (seq_noValue));
    }

}
