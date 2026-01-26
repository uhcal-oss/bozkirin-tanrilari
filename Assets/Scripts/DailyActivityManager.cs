using UnityEngine;

public class DailyActivityManager : MonoBehaviour
{
    public static DailyActivityManager Instance;

    [Header("Settings")]
    public int maxInteractionsPerDay = 1;
    
    [Header("Debug")]
    public int interactionsToday = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Load count (if needed, though usually reset on Wake)
        interactionsToday = PlayerPrefs.GetInt("DailyInteractions", 0);
        
        // Safety: If it's a fresh day (Morning check similar to ClassTable), we could reset?
        // But ClassTable already handles morning reset. 
        // Let's rely on BedSleep to call ResetDay().
    }

    public bool CanInteract()
    {
        if (interactionsToday < maxInteractionsPerDay)
        {
            return true;
        }
        return false;
    }

    public void ConsumeInteraction()
    {
        interactionsToday++;
        PlayerPrefs.SetInt("DailyInteractions", interactionsToday);
        PlayerPrefs.Save();
        Debug.Log($"[DailyActivity] Interaction used! ({interactionsToday}/{maxInteractionsPerDay})");
    }

    public void ResetDay()
    {
        interactionsToday = 0;
        PlayerPrefs.SetInt("DailyInteractions", 0);
        PlayerPrefs.Save();
        Debug.Log("[DailyActivity] Day Reset! Counters cleared.");
    }
}
