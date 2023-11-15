using System;
using System.Collections.Generic;
using Backend.Data;

namespace Backend.Services
{
    public interface ICustomDictionaryService
    {
        void ProcessCustomDictionary(Dictionary customDictionary);
        List<Dictionary> GetCustomDictionaries();
    }
}

