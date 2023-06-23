using System;
using System.Collections.Generic;
using System.IO;
using Backend.Data;

namespace Backend.Services;
public class WebVttExporter
{
    private readonly Stream _outputStream;

    public WebVttExporter(Stream outputStream)
    {
        _outputStream = outputStream;
    }

    public void ExportSpeechBubbles(List<SpeechBubble> speechBubbles)
    {
        string webVttContent = ConvertToWebVttFormat(speechBubbles);
        WriteToStream(webVttContent);
    }

    private string ConvertToWebVttFormat(List<SpeechBubble> speechBubbles)
    {
    }


    private void WriteToStream(string content)
    {
    }
}
