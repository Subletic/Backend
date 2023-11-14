using System;
using System.Collections.Generic;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

namespace Backend.Data
{
    public class Dictionary
    {
        public StartRecognitionMessage_TranscriptionConfig StartRecognitionMessageTranscriptionConfig  { get; set; }

        public Dictionary(StartRecognitionMessage_TranscriptionConfig startRecognitionMessageTranscriptionConfig)
        {
            StartRecognitionMessageTranscriptionConfig = startRecognitionMessageTranscriptionConfig;
        }
    }
}