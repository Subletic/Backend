using Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;
using System.Text;
using System.Text.Json;

namespace BackendTests.SpeechmaticsMessages;

public class EndOfStreamMessageTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    private static readonly string messageValue = "EndOfStream";

    [Test, Order(1)]
    // Valid constructor call must be accepted
    public void ValidConstruction_DoesntThrow()
    {
        Assert.DoesNotThrow (() => {
            var _ = new EndOfStreamMessage (12345);
        });
    }

    [Test, Order(2)]
    // Valid message must be accepted during deserialisation
    public void ValidMessage_DoesntThrow()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + messageValue + @"""",
            @"""last_seq_no"": 0",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.DoesNotThrow (() => {
            JsonSerializer.Deserialize<EndOfStreamMessage> (outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<EndOfStreamMessage> (outer.ToString(), jsonOptions));
    }

    [Test, Order(3)]
    // Wrong message type must be rejected during deserialisation
    public void WrongMessage_Throws()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + "Not" + messageValue + @"""",
            @"""last_seq_no"": 0",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.Throws<ArgumentException> (() => {
            JsonSerializer.Deserialize<EndOfStreamMessage> (outer.ToString(), jsonOptions);
        });
    }

    [Test, Order(4)]
    // Invalid lst_seq_no type must be rejected during deserialisation
    public void InvalidLastseqno_Throws()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + "Not" + messageValue + @"""",
            @"""last_seq_no"": -22",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.Throws<JsonException> (() => {
            JsonSerializer.Deserialize<EndOfStreamMessage> (outer.ToString(), jsonOptions);
        });
    }

    [Test]
    // Valid construction must report correct contents
    public void ValidConstruction_CorrectContent()
    {
        ulong last_seq_noValue = 12345;

        EndOfStreamMessage message = new EndOfStreamMessage (last_seq_noValue);

        Assert.That(message.message, Is.EqualTo (messageValue));
        Assert.That(message.last_seq_no, Is.EqualTo (last_seq_noValue));
    }

    [Test]
    // Valid message must report correct contents
    public void ValidMessage_CorrectContent()
    {
        var last_seq_noValue = 12345;

        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + messageValue + @"""",
            @"""last_seq_no"": " + last_seq_noValue,
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        EndOfStreamMessage message = JsonSerializer.Deserialize<EndOfStreamMessage> (outer.ToString(),
            jsonOptions)!;

        Assert.That(message.message, Is.EqualTo (messageValue));
        Assert.That(message.last_seq_no, Is.EqualTo (last_seq_noValue));
    }
}
