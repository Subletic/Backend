using Backend.Data.SpeechmaticsMessages;
using System.Text;
using System.Text.Json;

namespace BackendTests.SpeechmaticsMessages;

public class InfoMessageTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    private static readonly string messageValue = "Info";

    [Test, Order(1)]
    // Valid constructor call must be accepted
    public void ValidConstruction_DoesntThrow()
    {
        Assert.DoesNotThrow (() => {
            new InfoMessage (null,
                "recognition_quality",
                "Running recognition using a broadcast model quality.",
                "broadcast");
        });
    }

    [Test, Order(2)]
    // Valid message must be accepted during deserialisation
    public void ValidMessage_DoesntThrow()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + messageValue + @"""",
            @"""type"": ""recognition_quality""",
            @"""quality"": ""broadcast""",
            @"""reason"": ""Running recognition using a broadcast model quality.""",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.DoesNotThrow (() => {
            JsonSerializer.Deserialize<InfoMessage> (outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<InfoMessage> (outer.ToString(), jsonOptions));
    }

    [Test, Order(3)]
    // Wrong message type must be rejected during deserialisation
    public void WrongMessage_Throws()
    {
        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + "Not" + messageValue + @"""",
            @"""type"": ""recognition_quality""",
            @"""quality"": ""broadcast""",
            @"""reason"": ""Running recognition using a broadcast model quality.""",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.Throws<ArgumentException> (() => {
            JsonSerializer.Deserialize<InfoMessage> (outer.ToString(), jsonOptions);
        });
    }

    [Test]
    // Valid construction must report correct contents
    public void ValidConstruction_CorrectContent()
    {
        int codeValue = 123;
        string typeValue = "recognition_quality";
        string qualityValue = "broadcast";
        string reasonValue = "Running recognition using a broadcast model quality.";

        InfoMessage message = new InfoMessage (code: codeValue,
            type: typeValue,
            quality: qualityValue,
            reason: reasonValue);

        Assert.That(message.message, Is.EqualTo (messageValue));
        Assert.That(message.code, Is.EqualTo (codeValue));
        Assert.That(message.type, Is.EqualTo (typeValue));
        Assert.That(message.quality, Is.EqualTo (qualityValue));
        Assert.That(message.reason, Is.EqualTo (reasonValue));
    }

    [Test]
    // Valid message must report correct contents
    public void ValidMessage_CorrectContent()
    {
        int? codeValue = null;
        string typeValue = "recognition_quality";
        string qualityValue = "telephony";
        string reasonValue = "Running recognition using a telephony model quality.";

        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + messageValue + @"""",
            // code is null
            @"""type"": """ + typeValue + @"""",
            @"""quality"": """ + qualityValue + @"""",
            @"""reason"": """ + reasonValue + @"""",
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        InfoMessage message = JsonSerializer.Deserialize<InfoMessage> (outer.ToString(),
            jsonOptions)!;

        Assert.That(message.message, Is.EqualTo (messageValue));
        Assert.That(message.code, Is.EqualTo (codeValue));
        Assert.That(message.type, Is.EqualTo (typeValue));
        Assert.That(message.quality, Is.EqualTo (qualityValue));
        Assert.That(message.reason, Is.EqualTo (reasonValue));
    }
}
