using UnityEngine;

public class HeartManager : MonoBehaviour
{
    public static HeartManager Instance;

    [Header("State")]
    // Tracks which specific hearts are collected (e.g. [0]=Labyrinth, [1]=Dino, [2]=Parkour)
    public bool[] heartsCollected = new bool[3];
    
    public int totalHeartsFound = 0;

    // UI State
    public bool IsUIOpen = false; // To block other inputs if needed

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Load Checks
        totalHeartsFound = 0;
        for (int i = 0; i < heartsCollected.Length; i++)
        {
            // Load from PlayerPrefs, e.g. "Heart_0"
            if (PlayerPrefs.GetInt($"Heart_{i}", 0) == 1)
            {
                heartsCollected[i] = true;
                totalHeartsFound++;
            }
        }
    }

    public void CollectHeart(int index)
    {
        if (index < 0 || index >= heartsCollected.Length) return;

        if (!heartsCollected[index])
        {
            heartsCollected[index] = true;
            totalHeartsFound++;
            
            PlayerPrefs.SetInt($"Heart_{index}", 1);
            PlayerPrefs.Save();
            
            Debug.Log($"[HeartManager] Collected Heart #{index}! Total: {totalHeartsFound}");
        }
    }

    public bool HasAllHearts()
    {
        return totalHeartsFound >= 3;
    }

    public void ResetProgress()
    {
         for (int i = 0; i < heartsCollected.Length; i++)
        {
            heartsCollected[i] = false;
            PlayerPrefs.SetInt($"Heart_{i}", 0);
        }
        totalHeartsFound = 0;
        PlayerPrefs.Save();
    }
}
