using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Attach to the player character (Rua). On left-click, performs a pixel-art
/// sword slash that angles toward the mouse cursor.
/// </summary>
public class SwordAttack : MonoBehaviour
{
    [Header("Sword Settings")]
    [Tooltip("How far from the player center the slash appears (in units).")]
    public float slashOffset = 4.0f;

    [Tooltip("Size of the slash sprite in world units.")]
    public float slashSize = 10.0f;

    [Tooltip("Duration of the full slash animation in seconds.")]
    public float slashDuration = 0.25f;

    [Tooltip("Cooldown between slashes in seconds.")]
    public float slashCooldown = 0.3f;

    [Tooltip("Color of the slash.")]
    public Color slashColor = Color.white;

    [Tooltip("Pixels per unit for the slash sprite (match your game's PPU).")]
    public int pixelsPerUnit = 16;

    [Tooltip("Width of the slash texture in pixels.")]
    public int slashPixelWidth = 24;

    [Tooltip("Height of the slash texture in pixels.")]
    public int slashPixelHeight = 8;

    [Header("Damage (Optional)")]
    public int damage = 1;
    public LayerMask hitLayers = ~0;

    [Header("Input")]
    public InputActionReference attackAction;

    [Header("Audio")]
    public AudioClip slashSound;
    [Range(0f, 1f)] public float slashVolume = 0.5f;

    private bool canSlash = true;
    private Camera mainCam;
    private AudioSource audioSource;
    private Vector2 cachedDirection = Vector2.right; // Updated every frame

    // Pre-generated slash frames (just one set — we rotate the GameObject)
    private Sprite[] slashFrames;

    void Start()
    {
        mainCam = Camera.main;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (attackAction != null)
        {
            attackAction.action.Enable();
            attackAction.action.performed += OnAttackPerformed;
        }

        GenerateSlashSprites();

        // Start disabled — BossFightManager will enable us
        enabled = false;
    }

    void OnDestroy()
    {
        if (attackAction != null)
        {
            attackAction.action.performed -= OnAttackPerformed;
        }
    }

    void Update()
    {
        // Find camera if we don't have one
        if (mainCam == null)
        {
            mainCam = Camera.main;
            // Fallback: find ANY camera if none is tagged MainCamera
            if (mainCam == null && Camera.allCamerasCount > 0)
            {
                Camera[] cams = Camera.allCameras;
                if (cams.Length > 0) mainCam = cams[0];
            }
        }

        // Cache direction to cursor every frame
        if (mainCam != null && Mouse.current != null)
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector2 playerScreen = (Vector2)mainCam.WorldToScreenPoint(transform.position);
            Vector2 dir = (mouseScreen - playerScreen);

            if (dir.sqrMagnitude > 1f)
            {
                cachedDirection = dir.normalized;
            }
        }
        else if (Mouse.current != null)
        {
            // Ultra-fallback: use screen center as player position
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 dir = (mouseScreen - screenCenter);

            if (dir.sqrMagnitude > 1f)
            {
                cachedDirection = dir.normalized;
            }
        }

        if (attackAction == null)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && canSlash)
            {
                PerformSlash();
            }
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (canSlash)
        {
            PerformSlash();
        }
    }

    private void PerformSlash()
    {
        if (!canSlash) return;

        // Use the cached direction (updated every frame in Update)
        Vector2 direction = cachedDirection;

        Debug.Log($"[SwordAttack] Slash direction: {direction}, angle: {Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg}°");

        // Play sound
        if (slashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(slashSound, slashVolume);
        }

        // Show slash visual
        StartCoroutine(SlashVisualRoutine(direction));

        // Damage detection
        DetectHits(direction);

        // Cooldown
        StartCoroutine(CooldownRoutine());
    }

    private void DetectHits(Vector2 direction)
    {
        Vector2 hitCenter = (Vector2)transform.position + direction * slashOffset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(hitCenter, new Vector2(slashSize, slashSize) * 0.8f, 0f, hitLayers);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            hit.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"[SwordAttack] Hit {hit.gameObject.name} for {damage} damage!");
        }
    }

    private Vector2 GetDirectionToCursor()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return Vector2.right;
        if (Mouse.current == null) return Vector2.right;

        // Compute direction in SCREEN SPACE — avoids all z-depth / camera issues
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 playerScreen = (Vector2)mainCam.WorldToScreenPoint(transform.position);

        Vector2 dir = (mouseScreen - playerScreen).normalized;

        if (dir.sqrMagnitude < 0.01f)
            return Vector2.right;

        return dir;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return transform.position + Vector3.right;

        if (Mouse.current == null)
        {
            return transform.position + Vector3.right;
        }

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z - transform.position.z);

        return mainCam.ScreenToWorldPoint(mouseScreenPos);
    }

    private IEnumerator SlashVisualRoutine(Vector2 direction)
    {
        // Create sprite object for the slash
        GameObject slashObj = new GameObject("PixelSlash");
        SpriteRenderer sr = slashObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 100;
        sr.color = slashColor;

        // Position: offset from player in cursor direction
        slashObj.transform.position = transform.position + (Vector3)(direction * slashOffset);

        // Rotate: the line is perpendicular to direction (like a sword swipe across)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        slashObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Scale to desired size
        float spriteWorldWidth = (float)slashPixelWidth / pixelsPerUnit;
        float scale = slashSize / spriteWorldWidth;
        slashObj.transform.localScale = new Vector3(scale, scale, 1f);

        // Animate through 3 frames
        int frameCount = slashFrames.Length;
        float frameTime = slashDuration / frameCount;

        for (int f = 0; f < frameCount; f++)
        {
            sr.sprite = slashFrames[f];

            // Follow the player
            slashObj.transform.position = transform.position + (Vector3)(direction * slashOffset);

            // Flash bright on first frame
            if (f == 0)
                sr.color = Color.white;
            else
                sr.color = slashColor;

            yield return new WaitForSeconds(frameTime);
        }

        // Quick fade
        float fadeTime = 0.06f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            sr.color = new Color(slashColor.r, slashColor.g, slashColor.b, a);
            slashObj.transform.position = transform.position + (Vector3)(direction * slashOffset);
            yield return null;
        }

        Destroy(slashObj);
    }

    /// <summary>
    /// Generates pixel-art slash sprites — a straight horizontal line, 3 frames.
    /// The GameObject rotation handles direction.
    /// </summary>
    private void GenerateSlashSprites()
    {
        slashFrames = new Sprite[3];

        int w = slashPixelWidth;
        int h = slashPixelHeight;

        // Frame 0: Thin line (wind-up)
        Texture2D tex0 = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex0.filterMode = FilterMode.Point;
        ClearTexture(tex0);
        DrawFrame0(tex0, w, h);
        tex0.Apply();
        slashFrames[0] = Sprite.Create(tex0, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit);

        // Frame 1: Thick line (impact)
        Texture2D tex1 = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex1.filterMode = FilterMode.Point;
        ClearTexture(tex1);
        DrawFrame1(tex1, w, h);
        tex1.Apply();
        slashFrames[1] = Sprite.Create(tex1, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit);

        // Frame 2: Dithered line (dissipate)
        Texture2D tex2 = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex2.filterMode = FilterMode.Point;
        ClearTexture(tex2);
        DrawFrame2(tex2, w, h);
        tex2.Apply();
        slashFrames[2] = Sprite.Create(tex2, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    private void DrawFrame0(Texture2D tex, int w, int h)
    {
        // Thin 1px horizontal line
        Color c = Color.white;
        int midY = h / 2;

        for (int x = 2; x < w; x++)
        {
            tex.SetPixel(x, midY, c);
        }
    }

    private void DrawFrame1(Texture2D tex, int w, int h)
    {
        // Thick horizontal line (5px tall) — impact
        Color bright = Color.white;
        Color mid = new Color(1f, 1f, 1f, 0.85f);
        Color edge = new Color(1f, 1f, 1f, 0.5f);
        int midY = h / 2;

        for (int x = 0; x < w; x++)
        {
            tex.SetPixel(x, midY, bright);
            if (midY - 1 >= 0) tex.SetPixel(x, midY - 1, mid);
            if (midY + 1 < h) tex.SetPixel(x, midY + 1, mid);
            if (midY - 2 >= 0) tex.SetPixel(x, midY - 2, edge);
            if (midY + 2 < h) tex.SetPixel(x, midY + 2, edge);
        }
    }

    private void DrawFrame2(Texture2D tex, int w, int h)
    {
        // Broken/dithered horizontal line — dissipating
        Color c = new Color(1f, 1f, 1f, 0.6f);
        Color dim = new Color(1f, 1f, 1f, 0.3f);
        int midY = h / 2;

        for (int x = 0; x < w; x++)
        {
            if (x % 2 == 0)
                tex.SetPixel(x, midY, c);
            if (x % 3 == 0 && midY - 1 >= 0)
                tex.SetPixel(x, midY - 1, dim);
            if (x % 3 == 1 && midY + 1 < h)
                tex.SetPixel(x, midY + 1, dim);
        }
    }

    private void ClearTexture(Texture2D tex)
    {
        Color[] clear = new Color[tex.width * tex.height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;
        tex.SetPixels(clear);
    }

    private IEnumerator CooldownRoutine()
    {
        canSlash = false;
        yield return new WaitForSeconds(slashCooldown);
        canSlash = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, slashOffset);
    }
}
