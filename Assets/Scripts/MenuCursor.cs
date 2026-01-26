using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Added for TextMeshPro support

[DefaultExecutionOrder(100)] // Ensures this runs AFTER buttons move (MenuSlideIn)
public class MenuCursor : MonoBehaviour
{
    [Header("Settings")]
    public RectTransform cursorVisual; // Assign the white image here
    public float moveSpeed = 10f;
    public float resizeSpeed = 10f;
    [Header("Visual Tweaks")]
    public Vector2 padding = new Vector2(10f, 5f); // Restored padding
    public bool putBehindButtons = false; // CHANGED: Draw on TOP to avoid being hidden by Panels
    public bool disableRaycast = true;   // Ensures it doesn't block clicks

    [Header("Sizing Options")]
    public bool matchWidth = true;  // If false, uses fixedSize.x
    public bool matchHeight = true; // If false, uses fixedSize.y
    public Vector2 fixedSize = new Vector2(200f, 50f);

    private Vector3 currentVelocityPos;
    private Vector2 currentVelocitySize;

    public static MenuCursor Instance { get; private set; }
    private Image cursorImage;
    private bool snapMode = false; // If true, movement is instant
    private GameObject lastSelectedObject; // Prevent losing track during animation
    private GameObject forcedTarget; // Explicit override (e.g. on click)

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (cursorVisual != null)
        {
            cursorImage = cursorVisual.GetComponent<Image>();
            if (cursorImage != null && disableRaycast) cursorImage.raycastTarget = false;

            if (putBehindButtons)
            {
                // In Unity UI, Top of Hierarchy = Back of Screen (Draws first)
                cursorVisual.SetAsFirstSibling();
            }
            else
            {
                // Draw on TOP (Last Sibling)
                cursorVisual.SetAsLastSibling();
            }

            // Ensure we start VISIBLE (Alpha ~75/255)
            if (cursorImage != null)
            {
                Color c = cursorImage.color;
                if (c.a < 0.1f) // If almost invisible
                {
                    c.a = 75f / 255f; // Set to default low alpha
                    cursorImage.color = c;
                    FileLogger.Log("[MenuCursor] Fixed invisible cursor alpha.");
                }
            }
        }
    }

    void Update()
    {
        // 1. Determine Target
        GameObject selectedObj = forcedTarget;

        // Auto-release forced target if it's disabled/hidden
        if (forcedTarget != null && !forcedTarget.activeInHierarchy)
        {
            forcedTarget = null;
            snapMode = false; // RESTORE SMOOTH MOVEMENT
            selectedObj = null;
        }
        
        if (selectedObj == null)
        {
            selectedObj = EventSystem.current.currentSelectedGameObject;
        }

        // DEBUG: Why is cursor not moving?
        if (selectedObj != lastSelectedObject)
        {
             // FileLogger.Log($"[MenuCursor] Selection Changed to: {selectedObj?.name}");
        }

        // VALIDATION: If the selected object isn't a button or text, ignore it.
        // This prevents the cursor from snapping to the Canvas or random non-interactables.
        if (selectedObj != null)
        {
            bool hasButton = selectedObj.GetComponent<Button>() != null;
            bool hasText = selectedObj.GetComponentInChildren<TMP_Text>() != null;
            
            if (!hasButton && !hasText)
            {
                FileLogger.Log($"[MenuCursor] IGNORING {selectedObj.name} (No Button or Text component found)");
                selectedObj = null; // Treat as invalid
            }
        }
            
        // If nothing valid is selected, stick to the last one
        if (selectedObj == null && lastSelectedObject != null)
        {
            selectedObj = lastSelectedObject;
        }
        else if (selectedObj != null)
        {
            lastSelectedObject = selectedObj;
        }

        if (selectedObj != null)
        {
            Vector2 targetSize = Vector2.zero;
            RectTransform targetRect = null; // Declared here

            // Try to find text inside the button for a tighter fit
            TMP_Text textComponent = selectedObj.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                // Use the text's RENDERED bounds (tight fit), not the blue box rect
                targetRect = textComponent.rectTransform;
                targetSize = textComponent.GetRenderedValues(false); 
            }
            else
            {
                // Fallback to the button itself
                targetRect = selectedObj.GetComponent<RectTransform>();
                if (targetRect) targetSize = targetRect.rect.size;
            }

            if (targetRect != null)
            {
                if (snapMode)
                {
                    cursorVisual.position = targetRect.position;
                    currentVelocityPos = Vector3.zero;
                }
                else
                {
                    // FIX: Use unscaledDeltaTime so cursor moves while paused
                    cursorVisual.position = Vector3.SmoothDamp(cursorVisual.position, targetRect.position, ref currentVelocityPos, 1f / moveSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
                }

                // 4. Calculate Final Size
                float finalW = matchWidth ? (targetSize.x + padding.x) : fixedSize.x;
                float finalH = matchHeight ? (targetSize.y + padding.y) : fixedSize.y;

                // 5. Resize
                if (snapMode)
                {
                    cursorVisual.sizeDelta = new Vector2(finalW, finalH);
                    currentVelocitySize = Vector2.zero;
                }
                else
                {
                    // FIX: Use unscaledDeltaTime
                    cursorVisual.sizeDelta = Vector2.SmoothDamp(cursorVisual.sizeDelta, new Vector2(finalW, finalH), ref currentVelocitySize, 1f / resizeSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
                }
                
                // Ensure it's visible
                if (!cursorVisual.gameObject.activeSelf) 
                {
                    cursorVisual.gameObject.SetActive(true);
                    FileLogger.Log("[MenuCursor] Cursor was disabled, enabling now.");
                }
            }
        } // Closes selectedObj != null
        else
        {
            // Optional: Hide if nothing is selected
            // cursorVisual.gameObject.SetActive(false);
        }
    }

    public void Flash()
    {
        if (cursorImage == null) return;
        StartCoroutine(FlashRoutine());
    }

    System.Collections.IEnumerator FlashRoutine()
    {
        Color baseColor = cursorImage.color;
        float lowAlpha = 75f / 255f;
        float highAlpha = 150f / 255f;

        for (int i = 0; i < 5; i++)
        {
            cursorImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, highAlpha);
            yield return new WaitForSecondsRealtime(0.05f); // Fast flash (UNSCALED)
            cursorImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, lowAlpha);
            yield return new WaitForSecondsRealtime(0.05f);
        }
        
        // Ensure we end up at the low alpha (resting state)
        cursorImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, lowAlpha);
    }

    public void SetSnapMode(bool active)
    {
        snapMode = active;
    }

    public void ForceFollow(GameObject target)
    {
        forcedTarget = target;
        snapMode = true; // Auto-enable snap when forcing
    }
}
