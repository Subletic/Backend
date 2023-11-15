using System;
using System.Collections.Generic;
using Backend.Data.SpeechmaticsMessages.StartRecognitionMessage.transcription_config;

namespace Backend.Data
{
    /**
     * <summary>
     * Klasse für ein Wörterbuch.
     * </summary>
     */
    public class Dictionary
    {
        /**
         * <summary>
         * Transkriptionskonfiguration des StartRecognitionMessage.
         * </summary>
         */
        public StartRecognitionMessage_TranscriptionConfig StartRecognitionMessageTranscriptionConfig { get; set; }

        /**
         * <summary>
         * Konstruktor für Dictionary.
         * </summary>
         * <param name="startRecognitionMessageTranscriptionConfig">Die Transkriptionskonfiguration des StartRecognitionMessage.</param>
         */
        public Dictionary(StartRecognitionMessage_TranscriptionConfig startRecognitionMessageTranscriptionConfig)
        {
            StartRecognitionMessageTranscriptionConfig = startRecognitionMessageTranscriptionConfig;
        }
    }
}
