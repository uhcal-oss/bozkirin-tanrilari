using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for EventSystem.current

public class RebindController : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionReference actionReference; // Drag "Move", "Jump" here (Sub-Action)
    [Tooltip("For WASD, Up=1, Down=2, Left=3, Right=4. Set -1 for normal buttons.")]
    public int bindingIndex = -1; // Specific binding slot to edit
    public string overrideName = ""; // E.g. "Move Up"

    [Header("UI")]
    public TMP_Text buttonText;
    public string parameterName = "Key"; // "Move Up"
    public GameObject waitingOverlay; // Optional "Press Any Key..." overlay

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    private Button myButton;

    void Start()
    {
        myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.AddListener(StartRebinding);
        }
        else Debug.LogError($"[RebindController] {name}: No Button component found!");

        if (actionReference == null)
        {
            Debug.LogError($"[RebindController] {name}: Action Reference is MISSING!");
            return;
        }

        // Load saved overrides
        string saveKey = GetSaveKey();
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            actionReference.action.LoadBindingOverridesFromJson(json);
        }

        UpdateDisplay();
    }

    string GetSaveKey()
    {
        // Unique key for every action + index combination
        return $"Binding_{actionReference.action.name}_{bindingIndex}";
    }

    void FinishRebinding()
    {
        int bindingIndexToSave = bindingIndex;
        if (bindingIndexToSave == -1) bindingIndexToSave = 0; // Default

        // 1. Save Logic
        string json = actionReference.action.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(GetSaveKey(), json);
        PlayerPrefs.Save();

        FileLogger.Log($"[Control] Rebound '{overrideName}' to {actionReference.action.bindings[bindingIndexToSave].effectivePath}");

        actionReference.action.Enable(); // Enable BEFORE Cleanup
        Cleanup();
    }

    void CancelRebinding()
    {
        actionReference.action.Enable(); // Enable BEFORE Cleanup
        Cleanup();
    }

    void Cleanup()
    {
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
            rebindingOperation = null;
        }

        if (waitingOverlay) waitingOverlay.SetActive(false);
        UpdateDisplay();
        
        // RESTORE FOCUS to this button so navigation continues!
        if (myButton != null)
        {
            StartCoroutine(RestoreFocus());
        }
    }

    System.Collections.IEnumerator RestoreFocus()
    {
        // Force Reset Selection
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame(); 
        
        FileLogger.Log($"[Rebind] Restoring focus to {name}");
        myButton.Select();
    }

    public void StartRebinding()
    {
        if (actionReference == null) return;
        if (rebindingOperation != null)
        {
            FileLogger.Log($"[Rebind] Ignored click: Rebind already in progress.");
            return;
        }

        try 
        {
            actions = actionReference.action;
            FileLogger.Log($"[Rebind] Starting rebind for: {actions.name}");

            // Check if bindings exist
            if (actions.bindings.Count == 0)
            {
                FileLogger.Log($"[Rebind] ERROR: Action {actions.name} has NO bindings to rebind!");
                if (buttonText) buttonText.text = $"ERR: No Bindings!";
                return;
            }

            if (buttonText) buttonText.text = $"{parameterName} < ... >";
            if (waitingOverlay) waitingOverlay.SetActive(true);

            // CRITICAL: Unselect the button so EventSystem doesn't eat the input!
            EventSystem.current.SetSelectedGameObject(null);

            actions.Disable();

            int targetIndex = bindingIndex;
            if (targetIndex == -1) targetIndex = 0;

            FileLogger.Log($"[Rebind] Target Binding Index: {targetIndex} (Total: {actions.bindings.Count})");

            if (targetIndex >= actions.bindings.Count)
            {
               FileLogger.Log($"[Rebind] ERROR: Index {targetIndex} is out of bounds!");
               CancelRebinding();
               return;
            }

            // Create Operation
            rebindingOperation = actions.PerformInteractiveRebinding(targetIndex);
            
            // Configure
            rebindingOperation
                .WithControlsExcluding("Mouse")
                .WithCancelingThrough("<Keyboard>/escape") 
                .OnMatchWaitForAnother(0.1f) 
                .OnPotentialMatch(match => FileLogger.Log($"[Rebind] Saw input: {match}"))
                .OnComplete(operation => 
                {
                    FileLogger.Log("[Rebind] Callback: Complete");
                    FinishRebinding();
                })
                .OnCancel(operation => 
                {
                    FileLogger.Log("[Rebind] Callback: Cancelled");
                    CancelRebinding();
                });

            // Start
            FileLogger.Log("[Rebind] Calling .Start()...");
            rebindingOperation.Start();
            FileLogger.Log("[Rebind] .Start() returned. Waiting for input.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Rebind] CRITICAL ERROR: {e.Message}\n{e.StackTrace}");
            CancelRebinding();
        }
    }

    private InputAction actions; // Helper cache

    void UpdateDisplay()
    {
        if (buttonText == null) return;
        if (actionReference == null || actionReference.action == null) return;

        // Get the current display string (e.g. "W", "Space")
        int index = bindingIndex != -1 ? bindingIndex : 0;
        
        // FIXED: Don't show "Hold" or interactions, just the key name
        var options = InputBinding.DisplayStringOptions.DontIncludeInteractions;
        string displayString = actionReference.action.GetBindingDisplayString(index, options);

        // EXTRA FIX: If the user picked the Wrong Index (The "Vector2" parent instead of "Up"), 
        // Unity returns a huge mess like "W | Up / S | Down...". 
        // We detect this and show a hint.
        if (displayString.Contains("/") || displayString.Length > 10) 
        {
             // It's likely they selected the Composite Parent (Index 0) instead of the Child (Index 1)
             // But we display it anyway, just cleaned up slightly or truncated?
             // Actually, let's just trust the string but if it's huge, maybe just show the first character?
             // No, let's just Log it so they know.
             FileLogger.Log($"[Rebind] WARNING: Display string includes composite syntax: '{displayString}'. Check Binding Index!");
        }

        string finalText = $"{parameterName}   [{displayString}]";

        FileLogger.Log($"[Rebind] UpdateDisplay: {parameterName} -> {displayString}");
        
        // Check for ButtonHover conflict and use its API if present
        var hoverScript = GetComponent<ButtonHover>();
        if (hoverScript != null)
        {
            hoverScript.UpdateContent(finalText);
        }
        else
        {
            buttonText.text = finalText;
        }
    }
}
