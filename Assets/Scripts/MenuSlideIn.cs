using UnityEngine;

public class MenuSlideIn : MonoBehaviour
{
    public float slideSpeed = 5f;
    public float verticalOffset = 1000f; // Positive = Top, Negative = Bottom
    public bool animateOnStart = true;
    public float delay = 0f; // Optional start delay

    [Header("Manual Override")]
    public bool useExplicitTarget = false;
    public Vector2 explicitTargetPosition;

    private Vector2 originalAnchoredPos;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            FileLogger.Log($"[SlideIn] ERROR: {name} has no RectTransform! removing script.");
            Destroy(this);
            return;
        }

        // Capture the "Inspector" position (Pos X, Pos Y) exactly as set
        originalAnchoredPos = rectTransform.anchoredPosition;
        FileLogger.Log($"[SlideIn] {name} Awake. Captured Target AnchoredPos: {originalAnchoredPos}");
    }

    void OnEnable()
    {
        if (animateOnStart && rectTransform != null)
        {
            // Determine where we want to end up
            Vector2 finalTarget = useExplicitTarget ? explicitTargetPosition : originalAnchoredPos;

            // Start at the offset (Top) relative to the TARGET
            Vector2 startPos = finalTarget + new Vector2(0, verticalOffset);
            
            // Snap to start
            rectTransform.anchoredPosition = startPos;
            
            FileLogger.Log($"[SlideIn] {name} OnEnable.");
            FileLogger.Log($"   -> Anchors: {rectTransform.anchorMin} / {rectTransform.anchorMax}");
            FileLogger.Log($"   -> Teleported to: {startPos}. Sliding to: {finalTarget}");

            // Debug Parent
            RectTransform parentRect = transform.parent?.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                FileLogger.Log($"[SlideIn] PARENT ({parentRect.name}):");
                FileLogger.Log($"   -> Size: {parentRect.rect.width} x {parentRect.rect.height}");
                FileLogger.Log($"   -> Anchors: {parentRect.anchorMin} / {parentRect.anchorMax}");
                FileLogger.Log($"   -> Pos: {parentRect.anchoredPosition}");
            }
            
            // Slide to target
            PlaySlide(finalTarget);
        }
    }

    /// <summary>
    /// Slides the element back UP (or wherever the offset is)
    /// </summary>
    public void PlayExitAnimation()
    {
        if (rectTransform == null) return;
        
        Vector2 finalTarget = useExplicitTarget ? explicitTargetPosition : originalAnchoredPos;
        Vector2 exitTarget = finalTarget + new Vector2(0, verticalOffset);
        
        PlaySlide(exitTarget);
    }

    public void PlaySlide(Vector2 targetPos)
    {
        StopAllCoroutines();
        StartCoroutine(SlideRoutine(targetPos));
    }

    System.Collections.IEnumerator SlideRoutine(Vector2 target)
    {
        if (delay > 0) yield return new WaitForSecondsRealtime(delay);
        
        while (Vector2.Distance(rectTransform.anchoredPosition, target) > 0.1f)
        {
            // USE UNSCALED TIME: Allows UI to animate even when Time.timeScale = 0 (Pause Menu)
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, target, slideSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        rectTransform.anchoredPosition = target;
    }
}