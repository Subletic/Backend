using System;
namespace Backend.Data
{
    public class CustomDictionary
    {
        public string Language { get; set; }
        public List<CustomDictionaryEntry> AdditionalVocab { get; set; }
    }
}