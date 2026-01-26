using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro
using System.Collections;

public class ClassTableInteractable : MonoBehaviour
{
    [Header("Current State")]
    public int timeState = 0; // 0=Morning, 1=Lunch, 2=Night
    
    [Header("References")]
    public DayNightController dayNightController;
    public GameObject berkeNPC; 
    
    [Header("UI References (Duplicate SleepPanel)")]
    public CanvasGroup fadeGroup; // The canvas group for fading
    public TextMeshProUGUI timeText; // The clock text
    
    [Header("Time Settings")]
    public float morningTime = 0.25f;
    public float lunchTime = 0.5f;
    public float nightTime = 0.8f; 
    public float animationDuration = 2.0f;

    [Header("Dialogue/Feedback")]
    public GameObject lunchDialogue; 
    public GameObject nightDialogue; 

    void Start()
    {
        // AUTO-RESET: if the global time says it's Morning, we should reset our state to 0!
        // This handles "Safety" when coming back from Sleep
        if (dayNightController != null && dayNightController.currentTime < 0.5f)
        {
             timeState = 0;
             PlayerPrefs.SetInt("ClassTimeState", 0);
             Debug.Log("[ClassTable] Detected Morning time: Resetting State to 0.");
        }
        else
        {
             timeState = PlayerPrefs.GetInt("ClassTimeState", 0);
        }

        ApplyState(timeState, false); 
        
        if (fadeGroup != null) 
        {
            fadeGroup.alpha = 0;
            fadeGroup.blocksRaycasts = false;
            fadeGroup.gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        // 1. Check for "New Day" Reset (If we slept but didn't reload scene)
        if (dayNightController != null && dayNightController.currentTime < 0.5f && timeState >= 2)
        {
             Debug.Log("[ClassTable] It's Morning again! resetting state loops.");
             timeState = 0;
             PlayerPrefs.SetInt("ClassTimeState", 0);
             ApplyState(0, false);
        }

        Debug.Log($"[ClassTable] Interact called. Current State: {timeState}");

        // STOP if already Night (State 2)
        if (timeState >= 2)
        {
             // ...
            Debug.Log("[ClassTable] It is night. Interaction blocked.");
            if (nightDialogue != null) 
            {
                nightDialogue.GetComponent<SimpleDialogue>()?.TriggerDialogue();
            }
            else
            {
                // Fallback to Popups if no dialogue assigned
                if (PlayerFeedbackUI.Instance != null)
                    PlayerFeedbackUI.Instance.ShowMessage("School is closed for the night.");
                else
                    Debug.LogWarning("[ClassTable] No Night Dialogue assigned & No FeedbackUI found!");
            }
            return;
        }

        StartCoroutine(TransitionRoutine());
    }

    [ContextMenu("Reset State to Morning")]
    public void ResetState()
    {
        timeState = 0;
        PlayerPrefs.SetInt("ClassTimeState", 0);
        PlayerPrefs.Save();
        ApplyState(0, false);
        Debug.Log("[ClassTable] State Reset to 0 (Morning).");
    }

    IEnumerator TransitionRoutine()
    {
        // 1. FADE OUT (Visuals like BedSleep)
        if (fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(true); 
            fadeGroup.blocksRaycasts = true;
            float t = 0;
            while (t < 1.0f)
            {
                t += Time.deltaTime * 2.0f; 
                fadeGroup.alpha = t;
                yield return null;
            }
            fadeGroup.alpha = 1;
        }

        // 2. SHOW TEXT
        if (timeText != null)
        {
            timeText.gameObject.SetActive(true); 
        }

        // 3. ANIMATE TIME TEXT
        int nextState = timeState + 1;
        float startT = GetTimeForState(timeState);
        float endT = GetTimeForState(nextState);

        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;
            float currentVal = Mathf.Lerp(startT, endT, progress);
            
            if (timeText != null)
            {
                timeText.text = DayNightController.GetTimeDisplay(currentVal);
            }
            yield return null;
        }

        // 4. SET STATE & SAVE
        timeState = nextState;
        Debug.Log($"[ClassTable] Time set to State {timeState}");
        
        ApplyState(timeState, true);

        PlayerPrefs.SetInt("ClassTimeState", timeState);
        PlayerPrefs.Save();
        
        if (timeText != null) timeText.text = DayNightController.GetTimeDisplay(endT);
        
        yield return new WaitForSeconds(0.5f);

        // 5. FADE IN
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
            fadeGroup.gameObject.SetActive(false); 
        }

        // 6. TRIGGER DIALOGUE
        if (timeState == 1 && lunchDialogue != null) 
            lunchDialogue.GetComponent<SimpleDialogue>()?.TriggerDialogue();
        else if (timeState == 2 && nightDialogue != null)
            nightDialogue.GetComponent<SimpleDialogue>()?.TriggerDialogue();
    }
    
    float GetTimeForState(int s)
    {
        switch (s)
        {
            case 0: return morningTime;
            case 1: return lunchTime;
            case 2: return nightTime;
            default: return morningTime;
        }
    }

    void ApplyState(int state, bool animateTime)
    {
        float targetTime = GetTimeForState(state);
        
        // Berke Logic:
        // Morning (0): Visible
        // Lunch (1): Gone
        // Night (2): Gone 
        bool berkeActive = (state == 0); 

        if (dayNightController != null)
        {
            dayNightController.SetTime(targetTime);
        }

        if (berkeNPC != null)
        {
            berkeNPC.SetActive(berkeActive);
        }
    }
}
