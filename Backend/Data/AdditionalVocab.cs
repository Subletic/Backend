using System;
using System.Collections.Generic;

namespace Backend.Data
{
    /**
     * <summary>
     * Klasse für zusätzliches Vokabular.
     * </summary>
     */
    public class AdditionalVocab
    {
        /**
         * <summary>
         * Inhalt des zusätzlichen Vokabulars.
         * </summary>
         */
        public string Content { get; set; }

        /**
         * <summary>
         * Liste von ähnlichen Klängen (optional).
         * </summary>
         */
        public List<string>? SoundsLike { get; set; }

        /**
         * <summary>
         * Konstruktor für AdditionalVocab.
         * </summary>
         * <param name="content">Der Inhalt des zusätzlichen Vokabulars.</param>
         * <param name="soundsLike">Liste von ähnlichen Klängen (optional).</param>
         */
        public AdditionalVocab(string content, List<string>? soundsLike = null)
        {
            Content = content;
            SoundsLike = soundsLike ?? new List<string>();
        }
    }
}
