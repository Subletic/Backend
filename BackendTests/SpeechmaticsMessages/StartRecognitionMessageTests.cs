﻿using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;
using System.Text;
using System.Text.Json;

namespace BackendTests.SpeechmaticsMessages;

public class StartRecognitionMessage_AudioFormatTests
{
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    [Test, Order(1)]
    // Valid values must be accepted
    public void ValidValues_DoesntThrow()
    {
        // our defaults
        Assert.DoesNotThrow(() => new StartRecognitionMessage_AudioFormat());

        // raw type
        Assert.DoesNotThrow(() =>
        {
            var _ = new StartRecognitionMessage_AudioFormat(type: "raw",
                encoding: "pcm_f32le",
                sample_rate: 16000);
            _ = new StartRecognitionMessage_AudioFormat(type: "raw",
                encoding: "pcm_s16le",
                sample_rate: 96000);
            _ = new StartRecognitionMessage_AudioFormat(type: "raw",
                encoding: "mulaw",
                sample_rate: 182000);
        });

        // file type
        Assert.DoesNotThrow(() => new StartRecognitionMessage_AudioFormat(type: "file",
            encoding: null,
            sample_rate: null));
    }

    [Test, Order(2)]
    // Invalid values must throw
    public void InvalidValues_Throws()
    {
        // bad encoding
        Assert.Throws<ArgumentException>(() => new StartRecognitionMessage_AudioFormat(type: "raw",
            encoding: "somethingelse",
            sample_rate: 8000));

        // bad sample_rate
        Assert.Throws<ArgumentOutOfRangeException>(() => new StartRecognitionMessage_AudioFormat(type: "raw",
            encoding: "mulaw",
            sample_rate: 0));
    }

    [Test]
    // Valid construction must report correct contents
    public void ValidConstruction_CorrectContent()
    {
        string[] types = new string[] {
            "raw",
            "raw",
            "raw",
            "file"
        };

        string?[] encodings = new string?[] {
            "pcm_f32le",
            "pcm_s16le",
            "mulaw",
            null
        };

        string?[] encodingsFfmpeg = new string?[] {
            "f32le",
            "s16le",
            "mulaw",
            null // conversion throws for type "file"
        };

        int?[] sample_rates = new int?[] {
            78000,
            12345,
            32000,
            null
        };

        for (int i = 0; i < types.Length; ++i)
        {
            StartRecognitionMessage_AudioFormat at = new StartRecognitionMessage_AudioFormat(type: types[i],
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
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true };

    private const string messageValue = "StartRecognition";

    [Test, Order(1)]
    // Valid constructor call must be accepted
    public void ValidConstruction_DoesntThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            // our defaults
            var _ = new StartRecognitionMessage(null, null);

            // TODO explicit preferences
        });
    }

    [Test, Order(2)]
    // Valid message must be accepted during deserialisation
    public void ValidMessage_DoesntThrow()
    {
        var innerAudioFormat = new StringBuilder();
        innerAudioFormat.AppendJoin(", ", new string[] {
            @"""type"": ""raw""",
            @"""encoding"": ""pcm_f32le""",
            @"""sample_rate"": 96000",
        });

        var innerTranscriptionConfig = new StringBuilder();
        innerTranscriptionConfig.AppendJoin(", ", new string[] {
            @"""language"": ""de""",
            @"""enable_partials"": true",
            @"""additional_vocab"": ???",
        });

        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[] {
            @"""message"": """ + messageValue + @"""",
            @"""audio_format"": { " + innerAudioFormat.ToString() + " }",
            @"""transcription_config"": { " + innerTranscriptionConfig.ToString() + " }"
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.DoesNotThrow(() =>
        {
            JsonSerializer.Deserialize<StartRecognitionMessage>(outer.ToString(), jsonOptions);
        });
        Assert.NotNull(JsonSerializer.Deserialize<StartRecognitionMessage>(outer.ToString(), jsonOptions));
    }

    [Test, Order(3)]
    // Wrong message type must be rejected during deserialisation
    public void WrongMessage_Throws()
    {
        var innerAudioFormat = new StringBuilder();
        innerAudioFormat.AppendJoin(", ", new string[] {
            @"""type"": ""raw""",
            @"""encoding"": ""f32le""",
            @"""sample_rate"": 96000",
        });

        var innerTranscriptionConfig = new StringBuilder();
        innerTranscriptionConfig.AppendJoin(", ", new string[] {
            @"""language"": ""de""",
            @"""enable_partials"": true",
        });

        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[] {
            @"""message"": """ + "Not" + messageValue + @"""",
            @"""audio_format"": { " + innerAudioFormat.ToString() + " }",
            @"""transcription_config"": { " + innerTranscriptionConfig.ToString() + " }"
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        Assert.Throws<ArgumentException>(() =>
        {
            JsonSerializer.Deserialize<StartRecognitionMessage>(outer.ToString(), jsonOptions);
        });
    }

    [Test]
    // Valid construction must report correct contents
    public void ValidConstruction_CorrectContent()
    {
        StartRecognitionMessage_AudioFormat at = new StartRecognitionMessage_AudioFormat(type: "raw",
            encoding: "mulaw",
            sample_rate: 44100);

        StartRecognitionMessage_TranscriptionConfig tc = new StartRecognitionMessage_TranscriptionConfig(
            language: "en",
            enable_partials: false);

        StartRecognitionMessage message = new StartRecognitionMessage(audio_format: at,
            transcription_config: tc);

        Assert.That(message.message, Is.EqualTo(messageValue));
        Assert.That(message.audio_format, Is.EqualTo(at));
        Assert.That(message.transcription_config, Is.EqualTo(tc));
    }

    [Test]
    // Valid message must report correct contents
    public void ValidMessage_CorrectContent()
    {
        var innerAudioFormat = new StringBuilder();
        innerAudioFormat.AppendJoin(", ", new string[] {
            @"""type"": ""file""",
            // encoding is raw-specific
            @"""encoding"": null",
            // sample_rate is raw-specific
            @"""sample_rate"": null",
        });

        var innerTranscriptionConfig = new StringBuilder();
        innerTranscriptionConfig.AppendJoin(", ", new string[] {
            @"""language"": ""de""",
            // enable_partials is optional
            @"""enable_partials"": null",
        });

        var inner = new StringBuilder();
        inner.AppendJoin(", ", new string[] {
            @"""message"": """ + messageValue + @"""",
            @"""audio_format"": { " + innerAudioFormat.ToString() + " }",
            @"""transcription_config"": { " + innerTranscriptionConfig.ToString() + " }",
        });

        var outer = new StringBuilder();
        outer.AppendJoin(" ", new string[] {
            "{",
            inner.ToString(),
            "}"
        });

        StartRecognitionMessage message = JsonSerializer.Deserialize<StartRecognitionMessage>(outer.ToString(),
            jsonOptions)!;

        Assert.That(message.message, Is.EqualTo(messageValue));
        Assert.That(message.audio_format.type, Is.EqualTo("file"));
        Assert.That(message.audio_format.encoding, Is.EqualTo(null));
        Assert.That(message.audio_format.sample_rate, Is.EqualTo(null));
        Assert.That(message.transcription_config.language, Is.EqualTo("de"));
        Assert.That(message.transcription_config.enable_partials, Is.EqualTo(null));
    }
}
