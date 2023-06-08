using Backend.Data;

namespace Backend.Services;

/// <summary>
/// Service to manage the list of speech bubbles.
/// </summary>
public class SpeechBubbleListService : ISpeechBubbleListService
{
    /// <summary>
    /// Linked List that holds all SpeechBubbles.
    /// </summary>
    private readonly LinkedList<SpeechBubble> _speechBubbleList;

    /// <summary>
    /// Initializes new LinkedList on Startup.
    /// </summary>
    public SpeechBubbleListService()
    {
        _speechBubbleList = new LinkedList<SpeechBubble>();
    }

    /// <summary>
    /// Getter for LinkedList.
    /// </summary>
    /// <returns>The List of SpeechBubbles</returns>
    public LinkedList<SpeechBubble> GetSpeechBubbles()
    {
        return _speechBubbleList;
    }

    /// <summary>
    /// Adds a new SpeechBubble to the LinkedList.
    /// The SpeechBubble is added to the end of the list.
    /// </summary>
    /// <param name="speechBubble">The SpeechBubble to append to the List</param>
    public void AddNewSpeechBubble(SpeechBubble speechBubble)
    {
        _speechBubbleList.AddLast(speechBubble);
    }

    /// <summary>
    /// Deletes the oldest SpeechBubble from the LinkedList.
    /// </summary>
    public void DeleteOldestSpeechBubble()
    {
        _speechBubbleList.RemoveFirst();
    }

    /// <summary>
    /// Replaces a SpeechBubble in the LinkedList with a new SpeechBubble.
    /// The SpeechBubble with the same ID as the new SpeechBubble is replaced.
    /// The order of the LinkedList is preserved.
    /// </summary>
    /// <param name="speechBubble">The SpeechBubble that changed.</param>
    public void ReplaceSpeechBubble(SpeechBubble speechBubble)
    {
        if (_speechBubbleList.Count == 0)
        {
            _speechBubbleList.AddFirst(speechBubble);
            return;
        }

        var currentSpeechBubble = _speechBubbleList.First;

        while (currentSpeechBubble != null)
        {
            if (currentSpeechBubble.Value.Id == speechBubble.Id)
            {
                // Replace the object in the linked list
                _speechBubbleList.AddAfter(currentSpeechBubble, speechBubble);
                _speechBubbleList.Remove(currentSpeechBubble);

                return;
            }

            currentSpeechBubble = currentSpeechBubble.Next; // Move to the next node
        }
    }
}