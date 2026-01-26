using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AudioSettingsController : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler
{
    public enum VolumeType { Master, Music, SFX }

    [Header("Settings")]
    public VolumeType volumeType = VolumeType.Master;
    public string parameterName = "Volume";
    public int minVolume = 0;
    public int maxVolume = 100;
    public int step = 10; // Change by 10% per press

    [Header("UI References")]
    public TMP_Text valueText;
    [Header("Audio")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    private int currentVolume = 100;
    private bool isEditing = false;
    private Button myButton;

    void Start()
    {
        // Auto-setup AudioSource if missing
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 1. Load saved volume
        string key = GetPrefKey();
        currentVolume = PlayerPrefs.GetInt(key, 100);
        
        // ... (rest of Start)
        
        // Apply immediately on start
        ApplyVolume(currentVolume);

        if (valueText == null)
        {
            valueText = GetComponentInChildren<TMP_Text>();
        }

        UpdateDisplay();

        myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.AddListener(() => {
                ToggleEditMode(!isEditing);
            });
        }
    }

    // --- INPUT HANDLING (Arrows) ---

    public void OnMove(AxisEventData eventData)
    {
        if (isEditing)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    ChangeVolume(-step);
                    eventData.Use(); 
                    break;
                case MoveDirection.Right:
                    ChangeVolume(step);
                    eventData.Use(); 
                    break;
                case MoveDirection.Up:
                case MoveDirection.Down:
                    eventData.Use(); // Lock vertical nav while editing
                    break;
            }
        }
    }

    void Update()
    {
        // Exit on Enter/Escape
        if (isEditing)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleEditMode(false);
            }
        }
    }

    public void OnSubmit()
    {
        ToggleEditMode(!isEditing);
    }

    void ChangeVolume(int amount)
    {
        currentVolume = Mathf.Clamp(currentVolume + amount, minVolume, maxVolume);
        
        // Play Sound
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        ApplyVolume(currentVolume);
        UpdateDisplay();
        
        // Save automatically
        PlayerPrefs.SetInt(GetPrefKey(), currentVolume);
        PlayerPrefs.Save();
    }

    void ApplyVolume(int vol)
    {
        float normalized = vol / 100f; // 0.0 to 1.0

        if (volumeType == VolumeType.Master)
        {
            AudioListener.volume = normalized;
        }
        else if (volumeType == VolumeType.Music)
        {
            // TODO: Link to Music Manager or AudioMixer
            FileLogger.Log($"[Audio] Music Volume set to {vol}% (Requires AudioMixer implementation)");
        }
        else if (volumeType == VolumeType.SFX)
        {
            // TODO: Link to SFX Manager or AudioMixer
            FileLogger.Log($"[Audio] SFX Volume set to {vol}% (Requires AudioMixer implementation)");
        }
    }

    string GetPrefKey()
    {
        return $"Volume_{volumeType}";
    }

    void ToggleEditMode(bool active)
    {
        isEditing = active;
        if (isEditing)
        {
            if (valueText) valueText.color = Color.green;
        }
        else
        {
            if (valueText) valueText.color = Color.white;
        }
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (valueText == null) return;

        if (isEditing)
        {
            valueText.text = $"{parameterName} < {currentVolume}% >";
        }
        else
        {
            // Pad properly to avoid jumping
            valueText.text = $"{parameterName}   {currentVolume}%";
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (isEditing) ToggleEditMode(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Standard select behavior
    }
}
