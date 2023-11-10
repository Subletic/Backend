using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Services;

public class CustomDictionaryService
{
    private readonly List<CustomDictionary> _customDictionaries = new List<CustomDictionary>();

    public void ProcessCustomDictionary(CustomDictionary customDictionary)
    {
        // logik
        _customDictionaries.Add(customDictionary);
    }
}
