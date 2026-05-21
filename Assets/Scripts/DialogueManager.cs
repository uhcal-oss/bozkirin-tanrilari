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
    
    [Header("Font Settings")]
    public TMP_FontAsset customFont; // Assign your VCR_OSD font here!

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

        // Auto-generate Undertale UI if not assigned
        if (dialoguePanel == null)
        {
            GenerateUndertaleUI();
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void GenerateUndertaleUI()
    {
        // 1. Create Canvas
        GameObject canvasObj = new GameObject("Auto Dialogue Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // 2. Create Box
        dialoguePanel = new GameObject("Dialogue Box");
        dialoguePanel.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = dialoguePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = new Vector2(800, 200);
        panelRect.anchoredPosition = new Vector2(0, 30);

        Image panelImage = dialoguePanel.AddComponent<Image>();
        panelImage.color = Color.black;
        
        UnityEngine.UI.Outline outline = dialoguePanel.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(4, -4);

        // 3. Create Portrait
        GameObject portraitObj = new GameObject("Portrait Image");
        portraitObj.transform.SetParent(dialoguePanel.transform, false);
        RectTransform portraitRect = portraitObj.AddComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0, 0.5f);
        portraitRect.anchorMax = new Vector2(0, 0.5f);
        portraitRect.pivot = new Vector2(0, 0.5f);
        
        // 4:3 Ratio for portraits (e.g. 200 width, 150 height)
        portraitRect.sizeDelta = new Vector2(200, 150);
        portraitRect.anchoredPosition = new Vector2(30, 0); // 30 pixels padding from left
        
        portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.preserveAspect = true; // Ensure the drawing doesn't stretch!

        // 4. Create Name text
        GameObject nameObj = new GameObject("Name Text");
        nameObj.transform.SetParent(dialoguePanel.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(0, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.sizeDelta = new Vector2(300, 40);
        nameRect.anchoredPosition = new Vector2(260, -20); // 30(pad) + 200(portrait) + 30(inner pad) = 260

        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 28;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.TopLeft;
        if (customFont != null) nameText.font = customFont;

        // 5. Create text
        GameObject textObj = new GameObject("Dialogue Text");
        textObj.transform.SetParent(dialoguePanel.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(260, 20); // Aligned with Name Text
        textRect.offsetMax = new Vector2(-20, -60); // Lowered max to not overlap with Name Text

        dialogueText = textObj.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 32;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        dialogueText.enableWordWrapping = true;
        if (customFont != null) dialogueText.font = customFont;
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
        bool hasPortrait = line.portrait != null;

        if (portraitImage != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.gameObject.SetActive(hasPortrait);
        }

        // Adjust text position based on portrait existence
        if (dialogueText != null)
        {
            RectTransform textRect = dialogueText.GetComponent<RectTransform>();
            if (hasPortrait)
            {
                // Indent text to make room for portrait (200 width + 30 pad left + 30 pad right = 260)
                textRect.offsetMin = new Vector2(260, 20);
                if (nameText != null) nameText.GetComponent<RectTransform>().anchoredPosition = new Vector2(260, -20);
            }
            else
            {
                // Full width text (just 30 padding from left)
                textRect.offsetMin = new Vector2(30, 20);
                if (nameText != null) nameText.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, -20);
            }
        }

        if (nameText != null)
        {
            nameText.text = line.speakerName;
            nameText.gameObject.SetActive(!string.IsNullOrEmpty(line.speakerName));
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
