namespace Backend.Services
{
    using System.Collections.Generic;
    using Backend.Data;

    /// <summary>
    /// Interface for converting speech bubbles to SRT format.
    /// </summary>
    public interface ISrtConverter
    {
        /// <summary>
        /// Converts a list of speech bubbles to SRT format and writes the content to the output stream.
        /// </summary>
        /// <param name="speechBubbles">The list of speech bubbles to convert.</param>
        public void ConvertSpeechBubblesToSrt(List<SpeechBubble> speechBubbles);
    }
}
