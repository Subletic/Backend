using System;
using System.Collections.Generic;

namespace Backend.Data
{
    public class CustomDictionaryEntry
    {
        public string Content { get; set; }
        public List<string>? SoundsLike { get; set; }

        public CustomDictionaryEntry(string content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }
    }
}
