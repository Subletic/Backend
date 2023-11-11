using System;
using System.Collections.Generic;

namespace Backend.Data
{
    public class TranscriptionConfig
    {
        public string Language { get; set; }
        public List<AdditionalVocab> AdditionalVocab { get; set; }

        public TranscriptionConfig(string language, List<AdditionalVocab> additionalVocab)
        {
            if (additionalVocab.Count > 1000)
            {
                throw new ArgumentException("additionalVocab list cannot exceed 1000 elements.");
            }

            Language = language;
            AdditionalVocab = additionalVocab;
        }
    }
}
