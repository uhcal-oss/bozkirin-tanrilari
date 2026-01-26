using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Required for Image
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage; // The UI Image component to show the sprite
    public TextMeshProUGUI nameText; // Optional: To show speaker name

    private Queue<DialogueLine> sentences;
    private bool isDialogueActive = false;
    public bool IsDialogueActive => isDialogueActive;


    // Optional: Freeze player while talking
    private MonoBehaviour playerScript; // Stores generic player script
    
    // Callback for when current dialogue ends
    private UnityEngine.Events.UnityEvent currentEndCallback;
    private float dialogueStartTime; // Input buffer


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sentences = new Queue<DialogueLine>();
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueLine[] dialogueLines, UnityEngine.Events.UnityEvent onEnd = null)
    {
        if (isDialogueActive) return; // Prevent double trigger

        Debug.Log($"[DialogueManager] StartDialogue called with {dialogueLines.Length} lines. OnEnd event: {(onEnd != null ? "YES" : "NO")}");
        
        currentEndCallback = onEnd; // Store the event to call later

        isDialogueActive = true;
        dialogueStartTime = Time.unscaledTime; // Fix input bleed
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        sentences.Clear();
        foreach (DialogueLine line in dialogueLines)
        {
            sentences.Enqueue(line);
        }

        // Freeze Player
        playerScript = FindFirstObjectByType<PlayerController25D>() as MonoBehaviour;
        if (playerScript == null) playerScript = FindFirstObjectByType<UndertaleMovement>() as MonoBehaviour;
        
        if (playerScript != null) playerScript.enabled = false;

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = sentences.Dequeue();
        
        // Update UI
        if (portraitImage != null)
        {
            portraitImage.sprite = line.portrait;
            // Hide image if null?
            portraitImage.gameObject.SetActive(line.portrait != null);
        }

        if (nameText != null)
        {
            nameText.text = line.speakerName;
        }
        else
        {
            // Debugging why it might be empty
            // Debug.LogWarning("DialogueManager: NameText reference is missing in the Inspector!");
        }

        // Stop any existing typing coroutine if we were doing effect
        StopAllCoroutines();
        StartCoroutine(TypeSentence(line.text));
    }

    IEnumerator TypeSentence(string sentence)
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                // yield return new WaitForSeconds(0.02f); // Typewriter speed
                yield return null; // Very fast typing (1 char per frame)
            }
        }
    }

    public void EndDialogue()
    {
        Debug.Log("[DialogueManager] EndDialogue called!");
        isDialogueActive = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        
        if (playerScript != null) playerScript.enabled = true;

        // Trigger the Next Event (Chain)
        if (currentEndCallback != null)
        {
            Debug.Log("[DialogueManager] Invoking OnDialogueEnd event!");
            currentEndCallback.Invoke();
        }
        else
        {
            Debug.Log("[DialogueManager] No OnDialogueEnd event registered.");
        }
        currentEndCallback = null; // Clean up
    }

    void Update()
    {
        // Simple input to advance using New Input System
        if (isDialogueActive)
        {
            bool advance = false;
            
            // Prevent immediate skip if we just started talking (Input Bleed-through)
            if (Time.unscaledTime - dialogueStartTime < 0.2f) return;

            // Added Enter Key support
            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)) advance = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) advance = true;

            if (advance)
            {
                DisplayNextSentence();
            }
        }
    }
}
