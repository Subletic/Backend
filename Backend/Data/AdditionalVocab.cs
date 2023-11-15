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
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the list of similar sounds (optional).
    /// </summary>
    public List<string>? SoundsLike { get; set; }

    /// <summary>
    /// Constructor for AdditionalVocab.
    /// </summary>
    /// <param name="content">The content of the additional vocabulary.</param>
    /// <param name="soundsLike">List of similar sounds (optional).</param>
    public AdditionalVocab(string content, List<string>? soundsLike = null)
    {
        Content = content;
        SoundsLike = soundsLike ?? new List<string>();
    }
}
