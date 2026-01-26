using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject pausePanel; // Drag the Panel here
    
    [Header("Refined UI")]
    public GameObject visibleFirstButton; // Assign 'Resume' button here

    [Header("Input")]
    public InputActionReference pauseAction; // Assign 'Pause' or 'Escape'

    // ... (Existing code)

    [Header("Player Tracking")]
    public Transform playerTransform; // Drag Player here for saving position

    private bool isPaused = false;

    private bool wasPressedLastFrame = false;
    private float lastToggleTime = 0f;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        
        // AUTO-SETUP: Find Player
        if (playerTransform == null)
        {
            GameObject p = GameObject.Find("Player"); 
            if (p == null) p = GameObject.FindWithTag("Player"); 
            if (p != null) playerTransform = p.transform;
        }

        if (pauseAction != null)
        {
            pauseAction.action.Enable();
        }
    }

    void Update()
    {
        // MANUAL ONE-SHOT LOGIC to fix infinite toggle bug
        // Time.timeScale = 0 can sometimes mess up WasPressedThisFrame
        
        bool isPressedNow = false;

        if (pauseAction != null && pauseAction.action.IsPressed())
        {
            isPressedNow = true;
        }
        else if (Keyboard.current != null && Keyboard.current.escapeKey.isPressed)
        {
            isPressedNow = true;
        }

        // Global UI Block check
        if (HeartManager.Instance != null && HeartManager.Instance.IsUIOpen)
        {
            return; // Ignore pause input while inventory is open
        }

        // Only toggle if we just started pressing it (Rising Edge)
        if (isPressedNow && !wasPressedLastFrame)
        {
            TogglePause();
        }

        wasPressedLastFrame = isPressedNow;
    }

    void TogglePause()
    {
        if (Time.unscaledTime - lastToggleTime < 0.2f) return;
        lastToggleTime = Time.unscaledTime;

        isPaused = !isPaused;
        FileLogger.Log($"Pause Toggled: {isPaused}");

        if (isPaused)
        {
            Time.timeScale = 0f; 
            if (pausePanel != null) 
            {
                pausePanel.SetActive(true);
                ForceFixUIScale(); // CRASH FIX: Ensure UI is not scale 0
                // DebugUI(); 
                
                // CURSOR INTEGRATION: Select the first button
                if (visibleFirstButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(null); // Clear first
                    EventSystem.current.SetSelectedGameObject(visibleFirstButton);
                }
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Resume();
        }
    }

    void ForceFixUIScale()
    {
        if (pausePanel == null) return;
        // The ButtonContainer was showing Scale 0 in logs.
        // We force all children of the panel to Scale 1.
        foreach(Transform child in pausePanel.transform)
        {
            if (child.localScale == Vector3.zero)
            {
                child.localScale = Vector3.one;
                FileLogger.Log($"[UI FIX] Fixed zero-scale on {child.name}");
            }
        }
    }

    public void Resume()
    {
        FileLogger.Log("Resuming Game...");
        isPaused = false;
        Time.timeScale = 1f; 
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void QuickSave()
    {
        FileLogger.Log("Attempting Quick Save...");
        if (playerTransform == null)
        {
            FileLogger.Log("ERROR: Cannot Save - Player Not Found!");
            Debug.LogError("Cannot Save: Player Transform not assigned!");
            return;
        }

        PlayerPrefs.SetFloat("SaveX", playerTransform.position.x);
        PlayerPrefs.SetFloat("SaveY", playerTransform.position.y);
        PlayerPrefs.SetFloat("SaveZ", playerTransform.position.z);
        PlayerPrefs.SetInt("SavedScene", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.SetInt("HasSave", 1);
        PlayerPrefs.Save();
        
        FileLogger.Log($"Quick Save Complete at {playerTransform.position}");
    }

    public void LoadSave()
    {
        FileLogger.Log("Attempting Load Save...");
        if (PlayerPrefs.GetInt("HasSave", 0) == 0)
        {
            FileLogger.Log("No Save Data Found.");
            return;
        }

        float x = PlayerPrefs.GetFloat("SaveX");
        float y = PlayerPrefs.GetFloat("SaveY");
        float z = PlayerPrefs.GetFloat("SaveZ");
        
        FileLogger.Log($"Teleporting Player to {x}, {y}, {z}");

        if (playerTransform != null)
        {
            playerTransform.position = new Vector3(x, y, z);
        }
        
        Resume(); 
    }

    public void ExitToMainMenu()
    {
        FileLogger.Log("Exiting to Main Menu...");
        Time.timeScale = 1f; 
        SceneManager.LoadScene(0); 
    }

    void DebugUI()
    {
        if (pausePanel == null) return;
        FileLogger.Log($"[UI DEBUG] Panel '{pausePanel.name}' Active: {pausePanel.activeSelf}");
        
        foreach (Transform child in pausePanel.transform)
        {
            PrintChild(child, "  ");
        }
    }

    void PrintChild(Transform t, string indent)
    {
        string status = t.gameObject.activeSelf ? "ACTIVE" : "INACTIVE";
        string pos = t.localPosition.ToString();
        string scale = t.localScale.ToString();
        FileLogger.Log($"{indent}-> {t.name} [{status}] Pos:{pos} Scale:{scale}");
        
        foreach (Transform child in t)
        {
            PrintChild(child, indent + "  ");
        }
    }
}
