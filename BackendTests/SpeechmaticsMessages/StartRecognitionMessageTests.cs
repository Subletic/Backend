namespace BackendTests.SpeechmaticsMessages;

using System.Text;
using System.Text.Json;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

public class StartRecognitionMessage_AudioFormatTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    // Valid values must be accepted
    [Test]
    [Order(1)]
    public void ValidValues_DoesntThrow()
    {
        // our defaults
        Assert.DoesNotThrow(() => new StartRecognitionMessage_AudioFormat());

        // raw type
        Assert.DoesNotThrow(() =>
        {
            var startRecognitionMessage_AudioFormat = new StartRecognitionMessage_AudioFormat(
                type: "raw",
                encoding: "pcm_f32le",
                sample_rate: 16000);
            startRecognitionMessage_AudioFormat = new StartRecognitionMessage_AudioFormat(
                type: "raw",
                encoding: "pcm_s16le",
                sample_rate: 96000);
            startRecognitionMessage_AudioFormat = new StartRecognitionMessage_AudioFormat(
                type: "raw",
                encoding: "mulaw",
                sample_rate: 182000);
        });

        // file type
        Assert.DoesNotThrow(() => new StartRecognitionMessage_AudioFormat(
            type: "file",
            encoding: null,
            sample_rate: null));
    }

    // Invalid values must throw
    [Test]
    [Order(2)]
    public void InvalidValues_Throws()
    {
        // bad encoding
        Assert.Throws<ArgumentException>(() => new StartRecognitionMessage_AudioFormat(
            type: "raw",
            encoding: "somethingelse",
            sample_rate: 8000));

        // bad sample_rate
        Assert.Throws<ArgumentOutOfRangeException>(() => new StartRecognitionMessage_AudioFormat(
            type: "raw",
            encoding: "mulaw",
            sample_rate: 0));
    }

    // Valid construction must report correct contents
    [Test]
    public void ValidConstruction_CorrectContent()
    {
        string[] types = new string[]
        {
            "raw",
            "raw",
            "raw",
            "file",
        };

        string?[] encodings = new string?[]
        {
            "pcm_f32le",
            "pcm_s16le",
            "mulaw",
            null,
        };

        string?[] encodingsFfmpeg = new string?[]
        {
            "f32le",
            "s16le",
            "mulaw",
            null, // conversion throws for type "file"
        };

        int?[] sample_rates = new int?[]
        {
            78000,
            12345,
            32000,
            null,
        };

        for (int i = 0; i < types.Length; ++i)
        {
            StartRecognitionMessage_AudioFormat at = new StartRecognitionMessage_AudioFormat(
                type: types[i],
                encoding: encodings[i],
                sample_rate: sample_rates[i]);
            Assert.That(at.type, Is.EqualTo(types[i]));
            Assert.That(at.encoding, Is.EqualTo(encodings[i]));
            Assert.That(at.sample_rate, Is.EqualTo(sample_rates[i]));

            if (at.type == "raw")
            {
                Assert.That(at.GetEncodingInFFMpegFormat(), Is.EqualTo(encodingsFfmpeg[i]));
                Assert.That(at.GetCheckedSampleRate(), Is.EqualTo(sample_rates[i]));
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => at.GetEncodingInFFMpegFormat());
                Assert.Throws<InvalidOperationException>(() => at.GetCheckedSampleRate());
            }
        }
    }
}

public class StartRecognitionMessageTests
{
    private const string MESSAGE_VALUE = "StartRecognition";

    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    // Valid constructor call must be accepted
    [Test]
    [Order(1)]
    public void ValidConstruction_DoesntThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            // our defaults
            var startRecognitionMessage = new StartRecognitionMessage(null, null);

            // TODO explicit preferences
        });
    }

    // Valid message must be accepted during deserialisation
    [Test]
    [Order(2)]
    public void ValidMessage_DoesntThrow()
    {
        var innerAudioFormat = new StringBuilder();
        innerAudioFormat.AppendJoin(", ", new string[]
        {
            @"""type"": ""raw""",
            @"""encoding"": ""pcm_f32le""",
            @"""sample_rate"": 96000",
        });

        var innerTranscriptionConfig = new StringBuilder();
        innerTranscriptionConfig.AppendJoin(", ", new string[]
        {
            @"""language"": ""de""",
            @"""enable_partials"": true",
        });

        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + MESSAGE_VALUE + @"""",
            @"""audio_format"": { " + innerAudioFormat.ToString() + " }",
            @"""transcription_config"": { " + innerTranscriptionConfig.ToString() + " }",
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
            JsonSerializer.Deserialize<StartRecognitionMessage>(outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<StartRecognitionMessage>(outer.ToString(), jsonOptions));
    }

    // Wrong message type must be rejected during deserialisation
    [Test]
    [Order(3)]
    public void WrongMessage_Throws()
    {
        var innerAudioFormat = new StringBuilder();
        innerAudioFormat.AppendJoin(", ", new string[]
        {
            @"""type"": ""raw""",
            @"""encoding"": ""f32le""",
            @"""sample_rate"": 96000",
        });

        var innerTranscriptionConfig = new StringBuilder();
        innerTranscriptionConfig.AppendJoin(", ", new string[]
        {
            @"""language"": ""de""",
            @"""enable_partials"": true",
        });

        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + "Not" + MESSAGE_VALUE + @"""",
            @"""audio_format"": { " + innerAudioFormat.ToString() + " }",
            @"""transcription_config"": { " + innerTranscriptionConfig.ToString() + " }",
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
            JsonSerializer.Deserialize<StartRecognitionMessage>(outer.ToString(), jsonOptions);
        });
    }

    // Valid construction must report correct contents
    [Test]
    public void ValidConstruction_CorrectContent()
    {
        StartRecognitionMessage_AudioFormat at = new StartRecognitionMessage_AudioFormat(
            type: "raw",
            encoding: "mulaw",
            sample_rate: 44100);

        StartRecognitionMessage_TranscriptionConfig tc = new StartRecognitionMessage_TranscriptionConfig(
            language: "en",
            enable_partials: false);

        StartRecognitionMessage message = new StartRecognitionMessage(audio_format: at, transcription_config: tc);

        Assert.That(message.message, Is.EqualTo(MESSAGE_VALUE));
        Assert.That(message.audio_format, Is.EqualTo(at));
        Assert.That(message.transcription_config, Is.EqualTo(tc));
    }

    // Valid message must report correct contents
    [Test]
    public void ValidMessage_CorrectContent()
    {
        var innerAudioFormat = new StringBuilder();
        innerAudioFormat.AppendJoin(", ", new string[]
        {
            @"""type"": ""file""",
            @"""encoding"": null", // encoding is raw-specific
            @"""sample_rate"": null", // sample_rate is raw-specific
        });

        var innerTranscriptionConfig = new StringBuilder();
        innerTranscriptionConfig.AppendJoin(", ", new string[]
        {
            @"""language"": ""de""",
            @"""enable_partials"": null", // enable_partials is optional
        });

        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[]
        {
            @"""message"": """ + MESSAGE_VALUE + @"""",
            @"""audio_format"": { " + innerAudioFormat.ToString() + " }",
            @"""transcription_config"": { " + innerTranscriptionConfig.ToString() + " }",
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[]
        {
            "{",
            inner.ToString(),
            "}",
        });

        StartRecognitionMessage message = JsonSerializer.Deserialize<StartRecognitionMessage>(outer.ToString(), jsonOptions)!;

        Assert.That(message.message, Is.EqualTo(MESSAGE_VALUE));
        Assert.That(message.audio_format.type, Is.EqualTo("file"));
        Assert.That(message.audio_format.encoding, Is.EqualTo(null));
        Assert.That(message.audio_format.sample_rate, Is.EqualTo(null));
        Assert.That(message.transcription_config.language, Is.EqualTo("de"));
        Assert.That(message.transcription_config.enable_partials, Is.EqualTo(null));
    }
}
