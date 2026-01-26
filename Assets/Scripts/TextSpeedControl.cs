using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TextSpeedControl : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler
{
    [Header("UI References")]
    public TMP_Text valueText; // Assign the text component here
    public string parameterName = "Text Speed";

    [Header("Settings")]
    public float minValue = 0.5f;
    public float maxValue = 2.0f;
    public float step = 0.1f;
    public float defaultValue = 1.0f;

    private float currentValue;
    private bool isEditing = false;
    private Button myButton;
    private Navigation originalNavigation;

    void Start()
    {
        // LOAD SAVED VALUE (Default to 1.0)
        currentValue = PlayerPrefs.GetFloat("TextSpeed", defaultValue);
        
        if (valueText == null)
        {
            valueText = GetComponentInChildren<TMP_Text>();
            if (valueText == null) Debug.LogError($"{name}: Missing 'Value Text' assignment!");
        }

        UpdateDisplay(); // Will fail safely or log if text is missing

        myButton = GetComponent<Button>();
        if (myButton != null)
        {
            // Reliable way: Hook into the Button's standard click event
            myButton.onClick.AddListener(() => {
                ToggleEditMode(!isEditing);
            });
        }
    }

    // REMOVED Update() entirely

    // --- INTERACTION LOGIC ---

    public void OnMove(AxisEventData eventData)
    {
        if (isEditing)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    FileLogger.Log("[TextSpeed] OnMove: Left");
                    ChangeValue(-step);
                    eventData.Use(); // Mark as consumed
                    break;
                case MoveDirection.Right:
                    FileLogger.Log("[TextSpeed] OnMove: Right");
                    ChangeValue(step);
                    eventData.Use(); // Mark as consumed
                    break;
                case MoveDirection.Up:
                case MoveDirection.Down:
                    // Consume Up/Down while editing so we don't accidentally navigate away
                    // Or let it fall through if we want to allow exit?
                    // Let's consume it to "lock" the user in edit mode until they press Enter/Escape
                    eventData.Use(); 
                    break;
            }
        }
        // If not editing, do nothing (let default navigation happen)
    }

    void Update()
    {
        // Only check for Escape/Enter to exit edit mode
        if (isEditing)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape))
            {
                FileLogger.Log("[TextSpeed] Return/Escape Pressed -> Exiting Edit Mode");
                ToggleEditMode(false);
            }
        }
    }

    // --- INTERACTION LOGIC ---

    // Also support "Submit" key (Enter/Space) via the standard Button event
    public void OnSubmit()
    {
        ToggleEditMode(!isEditing);
    }
    
    // ...

    void ChangeValue(float amount)
    {
        float old = currentValue;
        currentValue = Mathf.Clamp(currentValue + amount, minValue, maxValue);
        
        // SAVE IMMEDIATELY
        PlayerPrefs.SetFloat("TextSpeed", currentValue);
        PlayerPrefs.Save();

        FileLogger.Log($"[TextSpeed] Changed Value: {old} -> {currentValue} (Saved)");
        UpdateDisplay();
    }

    void ToggleEditMode(bool active)
    {
        FileLogger.Log($"[TextSpeed] Toggle Edit Mode: {active}");
        isEditing = active;

        if (isEditing)
        {
            // Visual feedback (optional color change)
            valueText.color = Color.green; 
        }
        else
        {
            valueText.color = Color.white;
        }
        
        UpdateDisplay();
    }



    void UpdateDisplay()
    {
        if (valueText == null) return;

        // Example: "Text Speed < 1.0x >" when editing
        if (isEditing)
        {
            valueText.text = $"{parameterName} < {currentValue:0.0}x >";
        }
        else
        {
            valueText.text = $"{parameterName}   {currentValue:0.0}x";
        }
    }

    // Automatically stop editing if we click away or lose focus
    public void OnDeselect(BaseEventData eventData)
    {
        if (isEditing) ToggleEditMode(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Just standard selection behavior
    }
}
