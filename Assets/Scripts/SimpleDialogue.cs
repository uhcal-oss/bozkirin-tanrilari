using UnityEngine;

using UnityEngine.Events;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;
    public Sprite portrait; // The image to show
    public string speakerName; // Optional: "Berke", "Yeliz", etc.
}

public class SimpleDialogue : MonoBehaviour
{
    [Header("Conversation Data")]
    public DialogueLine[] conversation;

    [Header("Events")]
    [Tooltip("Triggered when this specific dialogue finishes.")]
    public UnityEvent onDialogueEnd;

    /// <summary>
    /// Call this method from the Interactable's OnInteract event in Inspector.
    /// </summary>
    public void TriggerDialogue()
    {
        Debug.Log($"[SimpleDialogue] TriggerDialogue called on {gameObject.name}!");
        
        if (DialogueManager.Instance != null)
        {
            // Pass the conversation AND the event to run when it finishes
            DialogueManager.Instance.StartDialogue(conversation, onDialogueEnd);
        }
        else
        {
            Debug.LogError("[SimpleDialogue] DialogueManager not found in scene!");
        }
    }
}
