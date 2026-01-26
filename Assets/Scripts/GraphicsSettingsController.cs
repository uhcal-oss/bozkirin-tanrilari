using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GraphicsSettingsController : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler
{
    public enum SettingType { WindowParams, Gamma }

    [Header("Configuration")]
    public SettingType settingType = SettingType.WindowParams;
    public string parameterName = "Window Mode";
    
    [Header("Gamma Settings")]
    public Image gammaOverlay; // Assign a black UI Panel here (Canvas Order: High)
    public int minGamma = 0;
    public int maxGamma = 100;
    public int gammaStep = 10;

    [Header("UI References")]
    public TMP_Text valueText;
    
    [Header("Audio")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    // Internal State
    private int currentGamma = 100;
    private int currentWindowModeIndex = 0; // 0=Fullscreen, 1=Borderless, 2=Windowed
    private bool isEditing = false;
    private Button myButton;

    // Window Modes
    private readonly FullScreenMode[] modes = new FullScreenMode[] 
    { 
        FullScreenMode.ExclusiveFullScreen, 
        FullScreenMode.FullScreenWindow, 
        FullScreenMode.Windowed 
    };
    
    private readonly string[] modeNames = new string[] 
    { 
        "Fullscreen", 
        "Borderless", 
        "Windowed" 
    };

    void Start()
    {
        // Auto-setup Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (valueText == null) valueText = GetComponentInChildren<TMP_Text>();

        // Load Settings
        if (settingType == SettingType.WindowParams)
        {
            currentWindowModeIndex = PlayerPrefs.GetInt("WindowModeIdx", 0);
            ApplyWindowMode(currentWindowModeIndex);
        }
        else if (settingType == SettingType.Gamma)
        {
            currentGamma = PlayerPrefs.GetInt("Gamma", 100);
            ApplyGamma(currentGamma);
        }

        UpdateDisplay();

        myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.AddListener(() => ToggleEditMode(!isEditing));
        }
    }

    // --- INPUT ---

    public void OnMove(AxisEventData eventData)
    {
        if (isEditing)
        {
            bool changed = false;
            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    if (settingType == SettingType.WindowParams) CycleWindowMode(-1);
                    else ChangeGamma(-gammaStep);
                    changed = true;
                    eventData.Use();
                    break;
                case MoveDirection.Right:
                    if (settingType == SettingType.WindowParams) CycleWindowMode(1);
                    else ChangeGamma(gammaStep);
                    changed = true;
                    eventData.Use();
                    break;
                case MoveDirection.Up:
                case MoveDirection.Down:
                    eventData.Use();
                    break;
            }

            if (changed && audioSource && clickSound)
            {
                audioSource.PlayOneShot(clickSound);
            }
        }
    }

    public void OnSubmit() => ToggleEditMode(!isEditing);

    void Update()
    {
        if (isEditing)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleEditMode(false);
            }
        }
    }

    // --- LOGIC ---

    void CycleWindowMode(int direction)
    {
        currentWindowModeIndex += direction;
        
        // Wrap around
        if (currentWindowModeIndex < 0) currentWindowModeIndex = modes.Length - 1;
        if (currentWindowModeIndex >= modes.Length) currentWindowModeIndex = 0;

        ApplyWindowMode(currentWindowModeIndex);
        UpdateDisplay();
        
        PlayerPrefs.SetInt("WindowModeIdx", currentWindowModeIndex);
        PlayerPrefs.Save();
    }

    void ApplyWindowMode(int index)
    {
        // Safety check
        if (index < 0 || index >= modes.Length) index = 0;
        Screen.fullScreenMode = modes[index];
        FileLogger.Log($"[Graphics] Set Window Mode: {modes[index]}");
    }

    void ChangeGamma(int amount)
    {
        currentGamma = Mathf.Clamp(currentGamma + amount, minGamma, maxGamma);
        ApplyGamma(currentGamma);
        UpdateDisplay();
        
        PlayerPrefs.SetInt("Gamma", currentGamma);
        PlayerPrefs.Save();
    }

    void ApplyGamma(int gamma)
    {
        if (gammaOverlay != null)
        {
            // 100 Gamma = 0 Alpha (Clear)
            // 0 Gamma = 0.8 Alpha (Almost Black) - tuned for expected darkening
            float alpha = Remap(gamma, 0, 100, 0.8f, 0f);
            FileLogger.Log($"[Graphics] Applying Gamma: {gamma}% -> Alpha: {alpha:0.00}");
            Color c = gammaOverlay.color;
            c.a = alpha;
            gammaOverlay.color = c;
        }
    }

    // --- DISPLAY ---

    void UpdateDisplay()
    {
        if (valueText == null) return;

        string valString = "";
        
        if (settingType == SettingType.WindowParams)
        {
            valString = modeNames[currentWindowModeIndex];
        }
        else
        {
            valString = $"{currentGamma}%";
        }

        if (isEditing)
        {
            valueText.text = $"{parameterName} < {valString} >";
            valueText.color = Color.green;
        }
        else
        {
            valueText.text = $"{parameterName}   {valString}";
            valueText.color = Color.white;
        }
    }

    void ToggleEditMode(bool active)
    {
        isEditing = active;
        UpdateDisplay();
    }

    // --- HELPERS ---

    float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public void OnDeselect(BaseEventData eventData) => ToggleEditMode(false);
    public void OnSelect(BaseEventData eventData) { }
}
