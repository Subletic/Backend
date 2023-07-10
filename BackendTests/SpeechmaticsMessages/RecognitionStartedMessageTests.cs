using Backend.Data.SpeechmaticsMessages;
using System.Text;
using System.Text.Json;

namespace BackendTests.SpeechmaticsMessages;

public class RecognitionStartedMessageTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    private static readonly string messageValue = "RecognitionStarted";

    [Test, Order(1)]
    // Valid constructor call must be accepted
    public void ValidConstruction_DoesntThrow()
    {
        Assert.DoesNotThrow (() => {
            var _ = new RecognitionStartedMessage ("somegreatid", new RecognitionStartedMessage_LanguagePackInfo (
                adapted: false,
                itn: true,
                language_description: "Something",
                word_delimiter: " ",
                writing_direction: "left-to-right"));
        });
    }

    [Test, Order(2)]
    // Valid message must be accepted during deserialisation
    public void ValidMessage_DoesntThrow()
    {
        var innerLanguagePackInfo = new StringBuilder();
        innerLanguagePackInfo.AppendJoin (", ", new string[] {
            @"""adapted"": true",
            @"""itn"": false",
            @"""language_description"": ""Someotherthing""",
            @"""word_delimiter"": "";""",
            @"""writing_direction"": ""right-to-left""",
        });

        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + messageValue + @"""",
            @"""id"": ""someevenbetterid""",
            @"""language_pack_info"": { " + innerLanguagePackInfo.ToString() + " }"
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.DoesNotThrow (() => {
            JsonSerializer.Deserialize<RecognitionStartedMessage> (outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<RecognitionStartedMessage> (outer.ToString(), jsonOptions));
    }

    [Test, Order(3)]
    // Wrong message type must be rejected during deserialisation
    public void WrongMessage_Throws()
    {
        var innerLanguagePackInfo = new StringBuilder();
        innerLanguagePackInfo.AppendJoin (", ", new string[] {
            @"""adapted"": true",
            @"""itn"": false",
            @"""language_description"": ""Someotherthing""",
            @"""word_delimiter"": "";""",
            @"""writing_direction"": ""right-to-left""",
        });

        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + "Not" + messageValue + @"""",
            @"""id"": ""someevenbetterid""",
            @"""language_pack_info"": { " + innerLanguagePackInfo.ToString() + " }"
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.Throws<ArgumentException> (() => {
            JsonSerializer.Deserialize<RecognitionStartedMessage> (outer.ToString(), jsonOptions);
        });
    }

    [Test]
    // Valid construction must report correct contents
    public void ValidConstruction_CorrectContent()
    {
        string id = "807670e9-14af-4fa2-9e8f-5d525c22156e";

        RecognitionStartedMessage_LanguagePackInfo lpi = new RecognitionStartedMessage_LanguagePackInfo (
            adapted: false,
            itn: true,
            language_description: "English",
            word_delimiter: " ",
            writing_direction: "left-to-right");

        RecognitionStartedMessage message = new RecognitionStartedMessage (id, lpi);

        Assert.That(message.message, Is.EqualTo (messageValue));
        Assert.That(message.id, Is.EqualTo (id));
        Assert.That(message.language_pack_info, Is.EqualTo (lpi));
    }

    [Test]
    // Valid message must report correct contents
    public void ValidMessage_CorrectContent()
    {
        var innerLanguagePackInfo = new StringBuilder();
        innerLanguagePackInfo.AppendJoin (", ", new string[] {
            @"""adapted"": false",
            @"""itn"": true",
            @"""language_description"": ""German""",
            @"""word_delimiter"": "";""",
            @"""writing_direction"": ""right-to-left""",
        });

        var inner = new StringBuilder();
        inner.AppendJoin (", ", new string[] {
            @"""message"": """ + messageValue + @"""",
            @"""id"": ""Pustekuchen""",
            @"""language_pack_info"": { " + innerLanguagePackInfo.ToString() + " }"
        });

        var outer = new StringBuilder();
        outer.AppendJoin (" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        RecognitionStartedMessage message = JsonSerializer.Deserialize<RecognitionStartedMessage> (outer.ToString(),
            jsonOptions)!;

        Assert.That(message.message, Is.EqualTo (messageValue));
        Assert.That(message.id, Is.EqualTo ("Pustekuchen"));
        Assert.That(message.language_pack_info.adapted, Is.EqualTo (false));
        Assert.That(message.language_pack_info.itn, Is.EqualTo (true));
        Assert.That(message.language_pack_info.language_description, Is.EqualTo ("German"));
        Assert.That(message.language_pack_info.word_delimiter, Is.EqualTo (";"));
        Assert.That(message.language_pack_info.writing_direction, Is.EqualTo ("right-to-left"));
    }
}
