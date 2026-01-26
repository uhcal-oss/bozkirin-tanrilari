using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public TMP_Text myText;
    public GameObject blackBar;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.cyan;
    public string arrowPrefix = "> ";

    private string originalText;

    void Awake()
    {
        if (myText == null)
        {
            myText = GetComponentInChildren<TMP_Text>();
        }

        // Save the original text (e.g. "START")
        if (myText != null)
        {
            originalText = myText.text;
            
            // Clean up accidental baked-in arrows from Edit mode or restarts
            if (originalText.StartsWith(arrowPrefix))
            {
                originalText = originalText.Substring(arrowPrefix.Length);
            }
        }
    }

    void Start()
    {
        // Hide the default ugly button background
        var bgImage = GetComponent<Image>();
        if (bgImage != null)
        {
            var c = bgImage.color;
            c.a = 0f;
            bgImage.color = c;
        }

        // Reset to clean state
        DoStateNormal();

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => {
                if (MenuCursor.Instance != null) 
                {
                    MenuCursor.Instance.Flash();
                }
            });
        }
    }

    // --- MOUSE LOGIC ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        // When mouse hovers, tell Unity to "Select" this button
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DoStateNormal();
    }

    // --- KEYBOARD LOGIC ---
    public void OnSelect(BaseEventData eventData)
    {
        DoStateHighlighted();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DoStateNormal();
    }

    // --- API FOR REBIND CONTROLLER ---
    public void UpdateContent(string newText)
    {
        originalText = newText;
        // Refresh current state to show new text immediately
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == gameObject)
        {
            DoStateHighlighted();
        }
        else
        {
            DoStateNormal();
        }
    }

    // --- VISUAL HELPERS ---
    void DoStateHighlighted()
    {
        // Safety: Don't highlight if we haven't captured original text yet
        if (string.IsNullOrEmpty(originalText)) return;

        if (myText != null)
        {
            // Always ensure we start with clean state logic
            // If text is already "Interact [E]", we want "> Interact [E]"
            if (!myText.text.StartsWith(arrowPrefix))
            {
                myText.text = arrowPrefix + originalText;
            }
            // EDGE CASE: If text changed externally, force it
            else if (!myText.text.Contains(originalText))
            {
                 myText.text = arrowPrefix + originalText;
            }
            
            myText.color = hoverColor;
        }
        if (blackBar) blackBar.SetActive(true);
    }

    void DoStateNormal()
    {
        if (string.IsNullOrEmpty(originalText)) return;

        if (myText != null)
        {
            myText.text = originalText;
            myText.color = normalColor;
        }
        if (blackBar) blackBar.SetActive(false);
    }
}