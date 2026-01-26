using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class BookshelfInteractable : MonoBehaviour
{
    [Header("Outro Settings")]
    public GameObject outroPanel;       // The panel with texts
    public float startDelay = 3.0f;     // Wait 3s before showing panel
    public float outroDuration = 5.0f;  // Time to read before Main Menu? Or wait for Input? 
    // User said "AFTER THE OUTRO IT WILL GO BACK TO MAIN MENU AUTOMATICALLY", implies a timer.
    public string mainMenuSceneName = "SampleScene"; // Make sure to set this!

    private bool isOutroPlaying = false;
    private float interactCooldown = 0f;

    [Header("UI Panel (Missing Hearts)")]
    public GameObject inventoryPanel; // The main panel to show/hide

    [Header("State Objects")]
    public GameObject notEnoughText;  // Text for 0, 1, 2 hearts
    public GameObject goOnText;       // Text for 3 hearts
    public GameObject fullHeartVisual; // The visual on the bookshelf (sticker)

    [Header("Heart Slots (UI)")]
    public Image heart0Slot;
    public Image heart1Slot;
    public Image heart2Slot;

    public void Interact()
    {
        Debug.Log("[Bookshelf] Interact() called!"); 
        
        if (interactCooldown > 0) return;
        interactCooldown = 0.5f; // Set Cooldown

        if (HeartManager.Instance == null) 
        {
            Debug.LogError("[Bookshelf] No HeartManager found! Create the HeartManager object.");
            return;
        }

        if (isOutroPlaying) return;

        // CHECK VICTORY
        if (HeartManager.Instance.HasAllHearts())
        {
            Debug.Log("[Bookshelf] Has All Hearts -> Playing Outro");
            StartCoroutine(PlayOutroRoutine());
            return;
        }

        // NORMAL INTERACTION (Missing Hearts)
        // Toggle logic
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            ClosePanel();
            return;
        }

        // Open Inventory Panel
        if (inventoryPanel != null) 
        {
            inventoryPanel.SetActive(true);
            HeartManager.Instance.IsUIOpen = true; 
        }
        else
        {
            Debug.LogError("[Bookshelf] Inventory Panel is NOT assigned in Inspector!");
        }

        // Show Missing State
        Debug.Log($"[Bookshelf] Missing hearts. Total Found: {HeartManager.Instance.totalHeartsFound}");
        
        if (PlayerFeedbackUI.Instance != null)
        {
            PlayerFeedbackUI.Instance.ShowMessage($"I only found {HeartManager.Instance.totalHeartsFound} pieces...");
        }
        
        if (notEnoughText != null) notEnoughText.SetActive(true);
        if (goOnText != null) goOnText.SetActive(false); 
        if (fullHeartVisual != null) fullHeartVisual.SetActive(false);
        
        // Show parts
        // Debugging the array
        bool[] status = HeartManager.Instance.heartsCollected;
        Debug.Log($"[Bookshelf] Heart Status: [0]={status.Length > 0 && status[0]}, [1]={status.Length > 1 && status[1]}, [2]={status.Length > 2 && status[2]}");

        UpdateHeartSlot(heart0Slot, status.Length > 0 && status[0]);
        UpdateHeartSlot(heart1Slot, status.Length > 1 && status[1]);
        UpdateHeartSlot(heart2Slot, status.Length > 2 && status[2]);
    }

    IEnumerator PlayOutroRoutine()
    {
        isOutroPlaying = true;
        Debug.Log("[Bookshelf] Starting OUTRO sequence!");

        // 1. LOCK INPUT
        var pc = FindFirstObjectByType<PlayerController25D>();
        if (pc != null) pc.enabled = false;
        
        var um = FindFirstObjectByType<UndertaleMovement>();
        if (um != null) um.enabled = false;

        // 2. SHOW INVENTORY (FULL HEART) - 5 Seconds
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            
            // Ensure visual state is correct (Full Heart)
            if (notEnoughText != null) notEnoughText.SetActive(false);
            if (goOnText != null) goOnText.SetActive(true);
            if (fullHeartVisual != null) fullHeartVisual.SetActive(true);
            UpdateHeartSlot(heart0Slot, false);
            UpdateHeartSlot(heart1Slot, false);
            UpdateHeartSlot(heart2Slot, false);
        }

        yield return new WaitForSeconds(5.0f); // "inventory for 5 seconds"

        // 3. SWITCH TO OUTRO PANEL
        if (inventoryPanel != null) inventoryPanel.SetActive(false); // Hide Inventory
        
        if (outroPanel != null)
        {
             outroPanel.SetActive(true);
             // If you want a fade, we'd need a CanvasGroup on the OutroPanel.
             // For now, immediate switch as requested "switch to OutroPanel".
        }

        // 4. WAIT FOR READING (10 Seconds)
        yield return new WaitForSeconds(10.0f); // "then 10 seconds"

        // 5. LOAD MAIN MENU
        Debug.Log("[Bookshelf] Outro finished. Loading Main Menu...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    void Update()
    {
        // Decrement timer
        if (interactCooldown > 0)
        {
            interactCooldown -= Time.deltaTime;
        }

        if (isOutroPlaying) return; 

        // Detect Close Input
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
           // Check Cooldown before processing Input
           if (interactCooldown > 0) return;

           // Use New Input System
           if ((Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) || 
               (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
           {
               ClosePanel();
           }
        }
    }
    
    void UpdateHeartSlot(Image slot, bool isCollected)
    {
        if (slot != null)
        {
            slot.gameObject.SetActive(isCollected);
        }
    }

    public void ClosePanel()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (HeartManager.Instance != null) HeartManager.Instance.IsUIOpen = false;
    }
}
