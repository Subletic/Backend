namespace BackendTests.SpeechmaticsMessages;

using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.audio_format;

public class StartRecognitionMessage_AudioFormatTests
{
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
