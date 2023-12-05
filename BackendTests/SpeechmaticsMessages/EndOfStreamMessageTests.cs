namespace BackendTests.SpeechmaticsMessages;

using System.Text;
using System.Text.Json;
using Backend.Data.SpeechmaticsMessages.EndOfStreamMessage;

public class EndOfStreamMessageTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    private static readonly string messageValue = "EndOfStream";

    // Valid constructor call must be accepted
    [Test]
    [Order(1)]
    public void ValidConstruction_DoesntThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            var endOfStreamMessage = new EndOfStreamMessage(12345);
        });
    }

    // Valid message must be accepted during deserialisation
    [Test]
    [Order(2)]
    public void ValidMessage_DoesntThrow()
    {
        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + messageValue + @"""",
            @"""last_seq_no"": 0",
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
            JsonSerializer.Deserialize<EndOfStreamMessage>(outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<EndOfStreamMessage>(outer.ToString(), jsonOptions));
    }

    // Wrong message type must be rejected during deserialisation
    [Test]
    [Order(3)]
    public void WrongMessage_Throws()
    {
        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + "Not" + messageValue + @"""",
            @"""last_seq_no"": 0",
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
            JsonSerializer.Deserialize<EndOfStreamMessage>(outer.ToString(), jsonOptions);
        });
    }

    // Invalid lst_seq_no type must be rejected during deserialisation
    [Test]
    [Order(4)]
    public void InvalidLastseqno_Throws()
    {
        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + "Not" + messageValue + @"""",
            @"""last_seq_no"": -22",
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
            JsonSerializer.Deserialize<EndOfStreamMessage>(outer.ToString(), jsonOptions);
        });
    }

    // Valid construction must report correct contents
    [Test]
    public void ValidConstruction_CorrectContent()
    {
        ulong last_seq_noValue = 12345;

        EndOfStreamMessage message = new EndOfStreamMessage(last_seq_noValue);

        Assert.That(message.message, Is.EqualTo(messageValue));
        Assert.That(message.last_seq_no, Is.EqualTo(last_seq_noValue));
    }

    // Valid message must report correct contents
    [Test]
    public void ValidMessage_CorrectContent()
    {
        var last_seq_noValue = 12345;

        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + messageValue + @"""",
            @"""last_seq_no"": " + last_seq_noValue,
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[]
        {
            "{",
            inner.ToString(),
            "}",
        });

        EndOfStreamMessage message = JsonSerializer.Deserialize<EndOfStreamMessage>(outer.ToString(), jsonOptions)!;

        Assert.That(message.message, Is.EqualTo(messageValue));
        Assert.That(message.last_seq_no, Is.EqualTo(last_seq_noValue));
    }
}
