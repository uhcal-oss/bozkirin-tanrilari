using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro
using System.Collections;

public class BedSleep : MonoBehaviour
{
    [Header("Settings")]
    public float minWakeTime = 0.25f; // 6:00 AM
    public float maxWakeTime = 0.46f; // 11:00 AM
    public float animationDuration = 3.0f; // How long the sleep lasts

    [Header("UI References")]
    public CanvasGroup fadeGroup; // The black panel
    public TextMeshProUGUI timeText; // The clock text

    // Assign "DayNightManager" logic here
    private DayNightController timeController;
    private bool isSleeping = false;
    private Interactable interactable;

    void Start()
    {
        // Auto-find if not assigned
        timeController = FindFirstObjectByType<DayNightController>();
        interactable = GetComponent<Interactable>();

        // Ensure UI is hidden at start
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0;
            fadeGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        // Automatically Disable interaction if it's DAY
        if (interactable != null && timeController != null)
        {
            // We NO LONGER lock it here, because we want to show a Feedback Message instead
            // when the player clicks it during the day.
        }
    }

    /// <summary>
    /// Call this via Interactable Event
    /// </summary>
    public void GoToSleep()
    {
        if (isSleeping) return; // Prevent double sleep

        // Redundant check (Safety)
        if (interactable != null && !interactable.enabled) return;
        
        StartCoroutine(SleepRoutine());
    }

    IEnumerator SleepRoutine()
    {
        isSleeping = true;
        FileLogger.Log("[BedSleep] Starting Sleep Sequence...");

        // 1. FADE OUT (To Black)
        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true; // Block input clicks
            float t = 0;
            while (t < 1.0f)
            {
                t += Time.deltaTime * 2.0f; // 0.5s fade
                fadeGroup.alpha = t;
                yield return null;
            }
            fadeGroup.alpha = 1;
        }

        // 2. TIME PASSAGE ANIMATION
        float startDayTime = 0;
        if (timeController != null) startDayTime = timeController.currentTime;

        // Calculate a random Wake Up time for tomorrow
        float wakeUpTime = Random.Range(minWakeTime, maxWakeTime); 
        
        // Setup Animation Variables
        float displayTime = startDayTime; 
        float targetDisplayTime = wakeUpTime + 1.0f; // Add 1.0 to represent "Tomorrow" because we wrap around
        
        // If we are sleeping in the morning (weird), ensure we animate forward not backward
        if (startDayTime > wakeUpTime) targetDisplayTime = wakeUpTime + 1.0f; 
        else targetDisplayTime = wakeUpTime; // If start is 0.1 and wake is 0.3, just 0.3

        float timer = 0;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;
            
            // Lerp the value
            float currentAnimValue = Mathf.Lerp(startDayTime, targetDisplayTime, progress);
            
            // Wrap for display (if > 1.0, minus 1.0)
            float displayValNormalized = currentAnimValue % 1.0f;

            // Update Text
            if (timeText != null)
            {
                timeText.text = DayNightController.GetTimeDisplay(displayValNormalized);
            }

            // Sync Game Time too? Maybe visually only.
            // Let's sync game time so the light changes while text runs (cool effect behind black screen?)
            // Actually black screen covers it.
            
            yield return null;
        }

        // 3. SET FINAL TIME
        if (timeController != null)
        {
            timeController.SetTime(wakeUpTime);
        }
        
        // Final Text Update
        if (timeText != null)
        {
            timeText.text = DayNightController.GetTimeDisplay(wakeUpTime);
        }

        // 4. RESET DAILY LIMIT
        if (DailyActivityManager.Instance != null)
        {
            DailyActivityManager.Instance.ResetDay();
        }

        // 5. RESET CLASS TABLE (Bring Berke Back)
        var table = FindFirstObjectByType<ClassTableInteractable>();
        if (table != null)
        {
            table.ResetState();
        }

        yield return new WaitForSeconds(0.5f); // Small pause at morning

        // 4. FADE IN (To Game)
        if (fadeGroup != null)
        {
            float t = 1.0f;
            while (t > 0.0f)
            {
                t -= Time.deltaTime * 2.0f; 
                fadeGroup.alpha = t;
                yield return null;
            }
            fadeGroup.alpha = 0;
            fadeGroup.blocksRaycasts = false;
        }

        isSleeping = false;
        FileLogger.Log("[BedSleep] Woke up!");
    }
}
