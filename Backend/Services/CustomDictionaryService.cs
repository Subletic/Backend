using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Services;

/// <summary>
/// Dienstklasse zur Verarbeitung von benutzerdefinierten Wörterbüchern.
/// </summary>
public class CustomDictionaryService
{
    private readonly List<CustomDictionary> _customDictionaries;

    /// <summary>
    /// Initialisiert eine neue Instanz der Klasse <see cref="CustomDictionaryService"/>.
    /// </summary>
    public CustomDictionaryService()
    {
        _customDictionaries = new List<CustomDictionary>();
    }

    /// <summary>
    /// Verarbeitet das benutzerdefinierte Wörterbuch, indem überprüft wird, ob es bereits existiert, und es entsprechend hinzugefügt oder aktualisiert wird.
    /// </summary>
    /// <param name="customDictionary">Das zu verarbeitende benutzerdefinierte Wörterbuch.</param>
    public void ProcessCustomDictionary(CustomDictionary customDictionary)
    {
        // Überprüfen Sie, ob das übertragene Wörterbuch bereits vorhanden ist
        var existingDictionary = _customDictionaries.Find(d =>
            d.AdditionalVocab.Any(entry => entry.Content == customDictionary.AdditionalVocab[0].Content)
        );

        if (existingDictionary != null)
        {
            // Fügen Sie die zusätzlichen Vokabeln dem vorhandenen Wörterbuch hinzu
            existingDictionary.AdditionalVocab.AddRange(customDictionary.AdditionalVocab);
        }
        else
        {
            // Erstellen Sie eine Kopie des übertragenen Wörterbuchs, um die Datenstruktur zu behalten
            var newDictionary = new CustomDictionary(
                language: customDictionary.Language,
                additionalVocab: new List<CustomDictionaryEntry>(customDictionary.AdditionalVocab)
            );

            // Fügen Sie das neue Wörterbuch hinzu
            _customDictionaries.Add(newDictionary);
        }
    }
}
