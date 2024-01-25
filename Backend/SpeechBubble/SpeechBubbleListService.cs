namespace Backend.SpeechBubble;

using Backend.Data;

/// <summary>
/// Service to manage the list of speech bubbles.
/// </summary>
public class SpeechBubbleListService : ISpeechBubbleListService
{
    /// <summary>
    /// Linked List that holds all SpeechBubbles.
    /// </summary>
    private readonly LinkedList<SpeechBubble> speechBubbleList;

    /// <summary>
    /// Initializes new LinkedList on Startup.
    /// </summary>
    public SpeechBubbleListService()
    {
        this.speechBubbleList = new LinkedList<SpeechBubble>();
    }

    /// <summary>
    /// Getter for LinkedList.
    /// </summary>
    /// <returns>The List of SpeechBubbles</returns>
    public LinkedList<SpeechBubble> GetSpeechBubbles()
    {
        return speechBubbleList;
    }

    /// <summary>
    /// Adds a new SpeechBubble to the LinkedList.
    /// The SpeechBubble is added to the end of the list.
    /// </summary>
    /// <param name="speechBubble">The SpeechBubble to append to the List</param>
    public void AddNewSpeechBubble(SpeechBubble speechBubble)
    {
        speechBubbleList.AddLast(speechBubble);
    }

    /// <summary>
    /// Deletes the oldest SpeechBubble from the LinkedList.
    /// </summary>
    public void DeleteOldestSpeechBubble()
    {
        speechBubbleList.RemoveFirst();
    }

    /// <summary>
    /// Replaces a SpeechBubble in the LinkedList with a new SpeechBubble.
    /// The SpeechBubble with the same ID as the new SpeechBubble is replaced.
    /// The order of the LinkedList is preserved.
    /// </summary>
    /// <param name="speechBubble">The SpeechBubble that changed.</param>
    public void ReplaceSpeechBubble(SpeechBubble speechBubble)
    {
        if (speechBubbleList.Count == 0)
        {
            speechBubbleList.AddFirst(speechBubble);
            return;
        }

        var currentSpeechBubble = speechBubbleList.First;

        while (currentSpeechBubble != null)
        {
            if (currentSpeechBubble.Value.Id == speechBubble.Id)
            {
                // Replace the object in the linked list
                var oldSpeechBubbleCreationTime = currentSpeechBubble.Value.CreationTime;
                speechBubble.CreationTime = oldSpeechBubbleCreationTime;

                speechBubbleList.AddAfter(currentSpeechBubble, speechBubble);
                speechBubbleList.Remove(currentSpeechBubble);
                return;
            }

            currentSpeechBubble = currentSpeechBubble.Next; // Move to the next node
        }
    }

    /// <summary>
    /// Clear the LinkedList.
    /// </summary>
    public void Clear()
    {
        speechBubbleList.Clear();
    }
}
