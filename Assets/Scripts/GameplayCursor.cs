using UnityEngine;

/// <summary>
/// Replaces the OS cursor with a tiny dot texture during gameplay.
/// Attach to any always-active GameObject in gameplay scenes (e.g. the Player or a Manager).
/// </summary>
public class GameplayCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    [Tooltip("Radius of the visible dot in pixels (1 = tiny single pixel dot).")]
    public int dotRadius = 1;

    [Tooltip("Color of the dot.")]
    public Color dotColor = Color.white;

    // The texture is always this size to avoid OS upscaling
    private const int TextureSize = 32;
    private Texture2D cursorTexture;

    void Awake()
    {
        CreateDotTexture();
        // Start disabled — BossFightManager will enable us
        enabled = false;
    }

    void OnEnable()
    {
        ApplyCursor();
    }

    void OnDisable()
    {
        // Restore default cursor when this is disabled/destroyed
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnDestroy()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        if (cursorTexture != null)
        {
            Destroy(cursorTexture);
        }
    }

    /// <summary>
    /// Creates a large transparent texture with a tiny dot in the center.
    /// This avoids OS upscaling artifacts.
    /// </summary>
    private void CreateDotTexture()
    {
        cursorTexture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        cursorTexture.filterMode = FilterMode.Point;

        // Fill entire texture with transparent
        Color[] pixels = new Color[TextureSize * TextureSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Draw a small dot at the center
        float center = (TextureSize - 1) / 2f;

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= dotRadius)
                {
                    pixels[y * TextureSize + x] = dotColor;
                }
            }
        }

        cursorTexture.SetPixels(pixels);
        cursorTexture.Apply();
    }

    private void ApplyCursor()
    {
        if (cursorTexture == null) return;

        // Hotspot is the center of the texture (where the dot is)
        Vector2 hotspot = new Vector2(TextureSize / 2f, TextureSize / 2f);
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        Cursor.visible = true;
    }
}
