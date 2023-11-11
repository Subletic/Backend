using System;
using System.Collections.Generic;

namespace Backend.Data
{
    public class Dictionary
    {
        public TranscriptionConfig TranscriptionConfig { get; set; }

        public Dictionary(TranscriptionConfig transcriptionConfig)
        {
            TranscriptionConfig = transcriptionConfig;
        }
    }
}