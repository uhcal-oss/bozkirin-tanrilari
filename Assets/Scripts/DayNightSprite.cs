using UnityEngine;

public class DayNightSprite : MonoBehaviour
{
    [Header("Sprite Assets")]
    public Sprite daySprite;
    public Sprite nightSprite;

    [Header("Settings")]
    [Tooltip("Cycle point where Night begins (0.0 - 1.0). Default 0.75")]
    public float nightStart = 0.75f;
    [Tooltip("Cycle point where Day begins (0.0 - 1.0). Default 0.25")]
    public float dayStart = 0.25f;

    private SpriteRenderer sr;
    private bool isNight = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            FileLogger.Log($"[DayNightSprite] Error: No SpriteRenderer on {name}");
        }
    }

    void OnEnable()
    {
        DayNightController.OnTimeChanged += HandleTimeChange;
    }

    void OnDisable()
    {
        DayNightController.OnTimeChanged -= HandleTimeChange;
    }

    void HandleTimeChange(float timePercent)
    {
        if (sr == null) return;

        // Determine State
        // Night usually spans from 0.75 -> 1.0 AND 0.0 -> 0.25
        bool shouldBeNight = (timePercent >= nightStart) || (timePercent < dayStart);

        if (shouldBeNight != isNight)
        {
            isNight = shouldBeNight;
            SwapSprite();
        }
    }

    void SwapSprite()
    {
        if (isNight && nightSprite != null)
        {
            sr.sprite = nightSprite;
        }
        else if (!isNight && daySprite != null)
        {
            sr.sprite = daySprite;
        }
    }
}
