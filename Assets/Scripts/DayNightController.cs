using UnityEngine;
using UnityEngine.Rendering.Universal; // For URP Light2D
using UnityEngine.UI; // For Canvas Fallback

public class DayNightController : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Duration of a full Day/Night cycle in seconds")]
    public float dayDuration = 120f; 
    [Range(0, 1)] 
    public float currentTime = 0.25f; // Start at morning (0.25)

    [Header("Lighting")]
    public bool enableLighting = false; // User requested "Image Change Only" for now
    public Gradient lightColor;
    public Light2D globalLight; 
    public Image overlayPanel; // Fallback if no 2D Lights

    // Event system for other scripts to listen to
    public static System.Action<float> OnTimeChanged;

    private float timeRate;

    void Start()
    {
        timeRate = 1.0f / dayDuration;
        
        // Auto-Setup Gradient if empty
        if (lightColor == null || lightColor.colorKeys.Length == 0)
        {
            SetupDefaultGradient();
        }
    }

    void Update()
    {
        // Advance time
        currentTime += Time.deltaTime * timeRate;
        
        // Loop cycle
        if (currentTime >= 1.0f)
            currentTime = 0.0f;

        // Update Lighting (Only if enabled)
        if (enableLighting)
        {
            Color currentColor = lightColor.Evaluate(currentTime);

            if (globalLight != null)
            {
                globalLight.color = currentColor;
            }
            
            if (overlayPanel != null)
            {
                overlayPanel.color = currentColor;
            }
        }

        // Notify Listeners (DayNightSprite)
        OnTimeChanged?.Invoke(currentTime);
    }

    /// <summary>
    /// Manually sets the time (0.0 - 1.0).
    /// </summary>
    public void SetTime(float newTime)
    {
        currentTime = Mathf.Clamp01(newTime);
        FileLogger.Log($"[DayNightController] Time manually set to {currentTime}");
    }

    /// <summary>
    /// Converts time (0.0-1.0) to a string like "08:30 AM"
    /// </summary>
    public static string GetTimeDisplay(float time01)
    {
        // 0.0 = 00:00 (Midnight)
        // 1.0 = 24:00 (Midnight)
        float totalMinutes = time01 * 1440f; // 24 * 60
        int hours = Mathf.FloorToInt(totalMinutes / 60f);
        int minutes = Mathf.FloorToInt(totalMinutes % 60f);

        string suffix = (hours >= 12) ? "PM" : "AM";
        
        // Convert to 12-hour format if desired
        int displayHour = hours;
        if (displayHour > 12) displayHour -= 12;
        if (displayHour == 0) displayHour = 12;

        return $"{displayHour:00}:{minutes:00} {suffix}";
    }

    void SetupDefaultGradient()
    {
        lightColor = new Gradient();

        // 0.0 = Midnight (Dark Blue)
        // 0.25 = Morning (Orange/Yellow)
        // 0.5 = Noon (White)
        // 0.75 = Evening (Red/Purple)
        // 1.0 = Midnight (Dark Blue)

        GradientColorKey[] colors = new GradientColorKey[5];
        colors[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.3f), 0.0f);
        colors[1] = new GradientColorKey(new Color(1.0f, 0.8f, 0.6f), 0.25f);
        colors[2] = new GradientColorKey(Color.white, 0.5f);
        colors[3] = new GradientColorKey(new Color(1.0f, 0.5f, 0.4f), 0.75f);
        colors[4] = new GradientColorKey(new Color(0.1f, 0.1f, 0.3f), 1.0f);

        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);

        lightColor.SetKeys(colors, alphas);
    }
}
