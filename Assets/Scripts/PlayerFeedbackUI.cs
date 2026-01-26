using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerFeedbackUI : MonoBehaviour
{
    public static PlayerFeedbackUI Instance;

    [Header("UI References")]
    [Tooltip("Assign a TextMeshProUGUI that is centered on the screen or above player")]
    public TextMeshProUGUI feedbackText;
    public CanvasGroup feedbackCanvasGroup;

    [Header("Settings")]
    public float displayDuration = 2.0f;
    public float fadeSpeed = 3.0f;
    
    [Header("Follow Settings")]
    public bool followPlayer = false; // Changed to false for fixed position
    public Vector3 offset = new Vector3(0, 2.5f, 0); // Above head
    private Transform playerTransform;

    private Coroutine currentRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (feedbackCanvasGroup != null)
        {
            feedbackCanvasGroup.alpha = 0;
            feedbackCanvasGroup.blocksRaycasts = false;
        }
    }

    void Start()
    {
        // 1. Try finding by Component (Most reliable)
        if (playerTransform == null)
        {
            var pc = FindFirstObjectByType<PlayerController25D>();
            if (pc != null) playerTransform = pc.transform;
            else 
            {
                var um = FindFirstObjectByType<UndertaleMovement>();
                if (um != null) playerTransform = um.transform;
            }
        }

        // 2. Fallback to Name/Tag
        if (playerTransform == null)
        {
            GameObject p = GameObject.Find("Yeliz");
            if (p == null) p = GameObject.FindWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (playerTransform != null) Debug.Log($"[PlayerFeedbackUI] Following Player: {playerTransform.name}");
        else Debug.LogError("[PlayerFeedbackUI] Could NOT find Player! Text will not move.");
    }

    void Update()
    {
        if (followPlayer && playerTransform != null && feedbackText != null && Camera.main != null)
        {
            // Convert World Position (Player) -> Screen Position (UI)
            Vector3 screenPos = Camera.main.WorldToScreenPoint(playerTransform.position + offset);
            feedbackText.transform.position = screenPos;
        }
    }

    public void ShowMessage(string message)
    {
        if (feedbackText == null) return;

        feedbackText.text = message;
        
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        // Fade In
        if (feedbackCanvasGroup != null)
        {
            while (feedbackCanvasGroup.alpha < 1f)
            {
                feedbackCanvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
                yield return null;
            }
            feedbackCanvasGroup.alpha = 1f;
        }

        // Wait
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade Out
        if (feedbackCanvasGroup != null)
        {
            while (feedbackCanvasGroup.alpha > 0f)
            {
                feedbackCanvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
                yield return null;
            }
            feedbackCanvasGroup.alpha = 0f;
        }
    }
}
