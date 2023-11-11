using System;
using System.Collections.Generic;

namespace Backend.Data
{
    public class AdditionalVocab
    {
        public string Content { get; set; }
        public List<string>? SoundsLike { get; set; }

        public AdditionalVocab(string content, List<string>? soundsLike = null)
        {
            Content = content;
            SoundsLike = soundsLike ?? new List<string>();
        }
    }
}


