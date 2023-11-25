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
    /// Adds a delay length to the service configuration for dictionary processing.
    /// </summary>
    /// <param name="dictionary">The dictionary for which the delay length is added.</param>
    /// <param name="delayLength">The delay length to be added.</param>
    void AddDelayLength(Dictionary dictionary, int delayLength);

    /// <summary>
    /// Gets the custom dictionaries.
    /// </summary>
    /// <returns>The custom dictionaries.</returns>
    List<Dictionary> GetCustomDictionaries();

    /// <summary>
    /// Gets the list of configuration data associated with the custom dictionaries.
    /// </summary>
    /// <returns>The list of configuration data.</returns>
    List<ConfigurationData> GetconfigurationDataList();
}
