using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Containers")]
    public GameObject categoryButtonsContainer; // The parent object holding "Game", "Audio" etc buttons.
    
    [Header("Panels")]
    public GameObject controlsPanel;
    public GameObject graphicsPanel;
    public GameObject audioPanel;

    [Header("Buttons")]
    public Button controlsButton;
    public Button graphicsButton;
    public Button audioButton;
    public Button backButton;

    [Header("Settings")]
    public string mainMenuSceneName = "SampleScene"; // SAFER than index!
    public GameObject firstSelectedButton; // Button to select when scene starts

    void Start()
    {
        FileLogger.Clear(); 
        FileLogger.Log("Settings Menu Started");

        // UNLOCK Local Cursor so user can click things!
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; 

        // 1. Assign Button Listeners
        if (controlsButton) controlsButton.onClick.AddListener(() => OpenPanel(controlsPanel));
        else Debug.LogError("Setup Error: 'Controls Button' is not assigned in the Inspector!");

        if (graphicsButton) graphicsButton.onClick.AddListener(() => OpenPanel(graphicsPanel));
        else Debug.LogError("Setup Error: 'Graphics Button' is not assigned in the Inspector!");

        if (audioButton) audioButton.onClick.AddListener(() => OpenPanel(audioPanel));
        else Debug.LogError("Setup Error: 'Audio Button' is not assigned in the Inspector!");
        
        if (backButton) backButton.onClick.AddListener(BackToMainMenu);

        // 2. Default State: Show Categories, Hide Panels
        CloseAllPanels();
        
        // 3. Select the first button for keyboard navigation
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    private GameObject lastSelectedObj;

    void Update()
    {
        // 1. Log Selection Changes (Cursor Movement)
        GameObject currentSel = EventSystem.current.currentSelectedGameObject;
        if (currentSel != lastSelectedObj)
        {
            FileLogger.Log($"[EventSystem] Selection Changed: '{lastSelectedObj?.name}' -> '{currentSel?.name}'");
            lastSelectedObj = currentSel;
        }

        // 2. DEBUG: Check what we are clicking on
        if (Input.GetMouseButtonDown(0))
        {
             GameObject clickedObj = EventSystem.current.currentSelectedGameObject;
             // Raycast check for UI
             PointerEventData pointerData = new PointerEventData(EventSystem.current)
             {
                 position = Input.mousePosition
             };
             
             System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
             EventSystem.current.RaycastAll(pointerData, results);
             
             if (results.Count > 0)
             {
                 FileLogger.Log($"[MouseClick] Hit UI: {results[0].gameObject.name} (Root: {results[0].gameObject.transform.root.name})");
             }
             else
             {
                 FileLogger.Log("[MouseClick] Clicked on NOTHING (UI Raycast missed)");
             }
        }
    }

    [Header("First Selection Per Panel")]
    public GameObject firstSelectedControls;
    public GameObject firstSelectedGraphics;
    public GameObject firstSelectedAudio;

    void OpenPanel(GameObject panelToOpen)
    {
        // 1. Hide the Main Category Buttons
        if (categoryButtonsContainer) categoryButtonsContainer.SetActive(false);

        // 2. Close other panels just in case
        if (controlsPanel) controlsPanel.SetActive(false);
        if (graphicsPanel) graphicsPanel.SetActive(false);
        if (audioPanel) audioPanel.SetActive(false);

        // 3. Open the requested panel
        if (panelToOpen) 
        {
            panelToOpen.SetActive(true);
            FileLogger.Log($"Opened Panel: {panelToOpen.name}");

            // 4. Select the correct first button for this panel
            GameObject targetBtn = null;
            if (panelToOpen == controlsPanel) targetBtn = firstSelectedControls;
            else if (panelToOpen == graphicsPanel) targetBtn = firstSelectedGraphics;
            else if (panelToOpen == audioPanel) targetBtn = firstSelectedAudio;

            if (targetBtn != null)
            {
                StartCoroutine(SelectButtonLater(targetBtn));
            }
            else
            {
                FileLogger.Log($"[SettingsMenu] WARNING: No 'First Selected' button assigned for panel {panelToOpen.name}! Cursor selection will fail.");
            }
        }
    }

    System.Collections.IEnumerator SelectButtonLater(GameObject btn)
    {
        // Wait for one frame to let the UI enable and Layout rebuild
        yield return null; 
        EventSystem.current.SetSelectedGameObject(null); // Clear first
        EventSystem.current.SetSelectedGameObject(btn); // Then set
    }

    public void ClosePanel()
    {
        FileLogger.Log("[SettingsMenu] Closing current panel -> Returning to Categories");

        // 1. Close all panels
        if (controlsPanel) controlsPanel.SetActive(false);
        if (graphicsPanel) graphicsPanel.SetActive(false);
        if (audioPanel) audioPanel.SetActive(false);

        // 2. Show the Main Category Buttons again
        if (categoryButtonsContainer) categoryButtonsContainer.SetActive(true);
        
        // 3. Reset selection to avoid "lost" cursor
        if (firstSelectedButton != null) 
        {
            StartCoroutine(SelectButtonLater(firstSelectedButton));
        }
    }
    
    void CloseAllPanels()
    {
        if (controlsPanel) controlsPanel.SetActive(false);
        if (graphicsPanel) graphicsPanel.SetActive(false);
        if (audioPanel) audioPanel.SetActive(false);
        
        if (categoryButtonsContainer) categoryButtonsContainer.SetActive(true);
    }

    void BackToMainMenu()
    {
        // Optional: Play sound or animation here
        FileLogger.Log($"[SettingsMenu] BackToMainMenu clicked. Loading Scene: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
