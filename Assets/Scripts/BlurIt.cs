using UnityEngine;
using UnityEngine.UI;

public class BlurIt : MonoBehaviour
{
    [Header("Settings")]
    public float defaultBlur = 0f;
    public string parameterName = "_BlurSize";

    private Material targetMaterial;
    private float currentBlur = 0f;

    void Start()
    {
        // Try to get material from SpriteRenderer OR Image
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Create a dedicated instance so we don't blur ALL objects sharing the material
            targetMaterial = sr.material; 
        }
        else
        {
            Image img = GetComponent<Image>();
            if (img != null)
            {
                targetMaterial = img.material;
            }
        }

        if (targetMaterial == null)
        {
            FileLogger.Log($"[BlurIt] Error: No Renderer found on {name}");
            enabled = false;
        }
        else
        {
            SetBlur(defaultBlur);
        }
    }

    // Public function to set blur amount (0.0 to 0.1 usually)
    public void SetBlur(float amount)
    {
        if (targetMaterial == null) return;
        
        currentBlur = amount;
        targetMaterial.SetFloat(parameterName, currentBlur);
    }
    
    // Helper to toggle between 0 and a value
    public void ToggleBlur(bool active)
    {
        SetBlur(active ? 0.005f : 0f);
    }
}
