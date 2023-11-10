using System;
using System.Collections.Generic;

namespace Backend.Data
{
    public class CustomDictionary
    {
        public string Language { get; set; }
        public List<CustomDictionaryEntry> AdditionalVocab { get; set; }

        public CustomDictionary(string language, List<CustomDictionaryEntry> additionalVocab)
        {
            Language = language ?? throw new ArgumentNullException(nameof(language));
            AdditionalVocab = additionalVocab ?? throw new ArgumentNullException(nameof(additionalVocab));
        }
    }
}
