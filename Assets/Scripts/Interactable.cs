using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [Header("Visuals")]
    [Tooltip("The Prompt Image (e.g. 'Press E'). Drag the child GameObject here.")]
    public GameObject promptIcon;

    [Header("Interaction")]
    [Tooltip("Function to call when player interacts.")]
    public UnityEvent onInteract;

    [Header("Input")]
    public InputActionReference interactAction; // Optional: If using Input System
    
    // Manual Lock control (Controlled by BedSleep or other scripts)
    public bool IsLocked 
    {
        get => _isLocked;
        set 
        {
            _isLocked = value;
            if (_isLocked)
            {
                // Force hide immediately
                isPlayerNearby = false;
                if (promptIcon != null) promptIcon.SetActive(false);
            }
        }
    }
    private bool _isLocked = false;

    private bool isPlayerNearby = false;

    void Start()
    {
        // Hide prompt by default
        if (promptIcon != null) promptIcon.SetActive(false);

        // DO NOT force isTrigger = true. 
        // We let the user decide if this is a ghost zone or a solid object with a child trigger.
    }

    void OnDisable()
    {
        isPlayerNearby = false;
        if (promptIcon != null) promptIcon.SetActive(false);
    }

    void Update()
    {
        if (IsLocked) return; // Stop everything if locked

        // Prevent interacting again while Dialogue is open (Fixes 'E' loop)
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return;

        if (isPlayerNearby)
        {
            // Check Input: Supports generic 'E' key OR Input System Action
            bool pressed = false;

            if (interactAction != null && interactAction.action != null && interactAction.action.WasPressedThisFrame())
            {
                pressed = true;
            }
            else if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                pressed = true;
            }

            if (pressed)
            {
                Interact();
            }
        }
    }

    [Header("Game Design")]
    public bool consumesDayActivity = false; // Set TRUE for NPCs

    void Interact()
    {
        // ACTIVITY CHECK
        if (consumesDayActivity)
        {
            if (DailyActivityManager.Instance != null && !DailyActivityManager.Instance.CanInteract())
            {
                Debug.Log($"[Interactable] Daily limit reached. Cannot interact with {name}");
                if (PlayerFeedbackUI.Instance != null) 
                    PlayerFeedbackUI.Instance.ShowMessage("I can only talk to one person today.");
                else
                    Debug.LogWarning("PlayerFeedbackUI missing!");
                
                return; // STOP
            }
        }

        FileLogger.Log($"[Interactable] Interacted with {name}");
        onInteract?.Invoke();

        // CONSUME (Only if we actually invoked, assuming success)
        // Wait, normally conversation happens. We should consume immediately?
        // Or should we wait for Dialogue End?
        // For simplicity: Consume on Start.
        if (consumesDayActivity && DailyActivityManager.Instance != null)
        {
            DailyActivityManager.Instance.ConsumeInteraction();
        }
    }

    // TRIGGER LOGIC (Requires Player to have Rigidbody2D + Collider2D)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsLocked) return;

        FileLogger.Log($"[Interactable] Trigger Enter: {other.name} (Tag: {other.tag})");
        
        if (IsPlayer(other.gameObject))
        {
            isPlayerNearby = true;
            if (promptIcon != null) promptIcon.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Don't check Locked here, we always want to clean up if they leave
        if (IsPlayer(other.gameObject))
        {
            isPlayerNearby = false;
            if (promptIcon != null) promptIcon.SetActive(false);
        }
    }

    // FALLBACK: If user uses Solid Colliders instead of Triggers
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsLocked) return;

        FileLogger.Log($"[Interactable] Collision Enter: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

        if (IsPlayer(collision.gameObject))
        {
            isPlayerNearby = true;
            if (promptIcon != null) promptIcon.SetActive(true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (IsPlayer(collision.gameObject))
        {
            isPlayerNearby = false;
            if (promptIcon != null) promptIcon.SetActive(false);
        }
    }

    bool IsPlayer(GameObject obj)
    {
        // Check Script components (Most reliable)
        if (obj.GetComponent<PlayerController25D>() != null || obj.GetComponent<UndertaleMovement>() != null) return true;

        // Check Tag OR Name (Fallback)
        return obj.CompareTag("Player") || obj.name.Contains("Player") || obj.name.Contains("Yeliz") || obj.name.Contains("Berke");
    }
}
