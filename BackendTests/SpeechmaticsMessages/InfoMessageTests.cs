namespace BackendTests.SpeechmaticsMessages;

using System.Text;
using System.Text.Json;
using Backend.Data.SpeechmaticsMessages.InfoMessage;

public class InfoMessageTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    private static readonly string messageValue = "Info";

    // Valid constructor call must be accepted
    [Test]
    [Order(1)]
    public void ValidConstruction_DoesntThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            var infoMessage = new InfoMessage(
                null,
                "recognition_quality",
                "Running recognition using a broadcast model quality.",
                "broadcast");
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
            @"""type"": ""recognition_quality""",
            @"""quality"": ""broadcast""",
            @"""reason"": ""Running recognition using a broadcast model quality.""",
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
            JsonSerializer.Deserialize<InfoMessage>(outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<InfoMessage>(outer.ToString(), jsonOptions));
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
            @"""type"": ""recognition_quality""",
            @"""quality"": ""broadcast""",
            @"""reason"": ""Running recognition using a broadcast model quality.""",
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
            JsonSerializer.Deserialize<InfoMessage>(outer.ToString(), jsonOptions);
        });
    }

    // Valid construction must report correct contents
    [Test]
    public void ValidConstruction_CorrectContent()
    {
        int codeValue = 123;
        string typeValue = "recognition_quality";
        string qualityValue = "broadcast";
        string reasonValue = "Running recognition using a broadcast model quality.";

        InfoMessage message = new InfoMessage(
            code: codeValue,
            type: typeValue,
            quality: qualityValue,
            reason: reasonValue);

        Assert.That(message.message, Is.EqualTo(messageValue));
        Assert.That(message.code, Is.EqualTo(codeValue));
        Assert.That(message.type, Is.EqualTo(typeValue));
        Assert.That(message.quality, Is.EqualTo(qualityValue));
        Assert.That(message.reason, Is.EqualTo(reasonValue));
    }

    // Valid message must report correct contents
    [Test]
    public void ValidMessage_CorrectContent()
    {
        int? codeValue = null;
        string typeValue = "recognition_quality";
        string qualityValue = "telephony";
        string reasonValue = "Running recognition using a telephony model quality.";

        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + messageValue + @"""",
            @"""type"": """ + typeValue + @"""", // code is null
            @"""quality"": """ + qualityValue + @"""",
            @"""reason"": """ + reasonValue + @"""",
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[]
        {
            "{",
            inner.ToString(),
            "}",
        });

        InfoMessage message = JsonSerializer.Deserialize<InfoMessage>(outer.ToString(), jsonOptions)!;

        Assert.That(message.message, Is.EqualTo(messageValue));
        Assert.That(message.code, Is.EqualTo(codeValue));
        Assert.That(message.type, Is.EqualTo(typeValue));
        Assert.That(message.quality, Is.EqualTo(qualityValue));
        Assert.That(message.reason, Is.EqualTo(reasonValue));
    }
}
