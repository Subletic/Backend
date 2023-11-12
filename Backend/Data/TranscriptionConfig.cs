using System;
using System.Collections.Generic;

namespace Backend.Data
{
    public class TranscriptionConfig
    {
        public string Language { get; set; }
        public List<AdditionalVocab> AdditionalVocab { get; set; }
        private const int MAX_ADDITIONAL_VOCAB_COUNT = 1000;
        public TranscriptionConfig(string language, List<AdditionalVocab> additionalVocab)
        {
            if (additionalVocab.Count > MAX_ADDITIONAL_VOCAB_COUNT)
            {
                throw new ArgumentException("additionalVocab list cannot exceed {MAX_ADDITIONAL_VOCAB_COUNT} elements.");
            }

            Language = language;
            AdditionalVocab = additionalVocab;
        }
    }
}
