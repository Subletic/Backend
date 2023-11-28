namespace Backend.Services;

using System;
using System.Collections.Generic;
using Backend.Data;

/// <summary>
/// Interface for the custom dictionary service.
/// </summary>
public interface ICustomDictionaryService
{
    /// <summary>
    /// Processes and adds a custom dictionary to the service for later use.
    /// </summary>
    /// <param name="customDictionary">The custom dictionary to process.</param>
    void ProcessCustomDictionary(Dictionary customDictionary);

    /// <summary>
    /// Gets the custom dictionaries.
    /// </summary>
    /// <returns>The custom dictionaries.</returns>
    List<Dictionary> GetCustomDictionaries();

    /// <summary>
    /// Sets the delay length for a specific operation.
    /// </summary>
    /// <param name="delay">The delay length to be set.</param>
    public void SetDelay(float delay);

    /// <summary>
    /// Retrieves the delay length for a specific operation.
    /// </summary>
    /// <returns>The delay length.</returns>
    public float GetDelay(); 
}
