#pragma warning disable IDE1006
#pragma warning disable SA1300
namespace Backend.Data;

using System;
using System.Collections.Generic;

/// <summary>
/// Class for additional vocabulary.
/// </summary>
public class AdditionalVocab
{
    /**
    * <summary>
    * Gets or sets the content of the additional vocabulary.
    * </summary>
    **/
    public string content { get; set; }

    /// <summary>
    /// Gets or sets the list of similar sounds (optional).
    /// </summary>
    public List<string>? sounds_like { get; set; }

    /// <summary>
    /// Constructor for AdditionalVocab.
    /// </summary>
    /// <param name="content">The content of the additional vocabulary.</param>
    /// <param name="sounds_like">List of similar sounds (optional).</param>
    public AdditionalVocab(string content, List<string>? sounds_like = null)
    {
        this.content = content;
        this.sounds_like = sounds_like ?? new List<string>();
    }
}
