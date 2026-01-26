using UnityEngine;
using UnityEngine.InputSystem;

public class LabyrinthPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 300f; // Speed for UI movement
    public RectTransform rectTransform;
    
    [Header("Audio")]
    public AudioClip[] footstepSounds;
    public float footstepInterval = 0.4f;

    [Header("Visuals (UI or Sprite)")]
    public UnityEngine.UI.Image targetImage; // If using Image (UI)
    public SpriteRenderer targetSprite; // If using SpriteRenderer (World)
    public Sprite[] moveUpSprites;
    public Sprite[] moveDownSprites;
    public Sprite[] moveLeftSprites;
    public Sprite[] moveRightSprites;
    public float animationSpeed = 0.15f;

    private float animTimer;
    private int animFrame;
    private Sprite[] currentAnim;

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private float footstepTimer;
    private AudioSource audioSource;
    private bool canMove = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (targetImage == null) targetImage = GetComponent<UnityEngine.UI.Image>();
        if (targetSprite == null) targetSprite = GetComponent<SpriteRenderer>();
        
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Ensure Physics settings for Top-Down UI
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        currentAnim = moveDownSprites; // Default
    }

    public void SetCanMove(bool val)
    {
        canMove = val;
    }

    void Update()
    {
        if (!canMove) return;

        // Read Input
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1;
        }

        moveInput = input.normalized;
        
        HandleAnimation();

        // 1. If we have a Rigidbody, let FixedUpdate handle physics (better for collisions)
        // 2. If NO Rigidbody but we have RectTransform, move UI manually
        if (rb == null && rectTransform != null)
        {
            rectTransform.anchoredPosition += moveInput * moveSpeed * Time.deltaTime;
        }

        // Audio
        if (moveInput.magnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                if (AudioManager.Instance != null && footstepSounds != null && footstepSounds.Length > 0)
                {
                    AudioManager.Instance.PlayRandomFootstep(footstepSounds, audioSource);
                }
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0.1f;
        }
    }

    void HandleAnimation()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            // Determine Direction
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                currentAnim = (moveInput.x > 0) ? moveRightSprites : moveLeftSprites;
            }
            else
            {
                currentAnim = (moveInput.y > 0) ? moveUpSprites : moveDownSprites;
            }

            // Animate
            animTimer += Time.deltaTime;
            if (animTimer >= animationSpeed)
            {
                animTimer = 0;
                animFrame++;
            }
        }
        else
        {
            animFrame = 0; // Reset to idle frame
        }

        // Apply Sprite
        if (currentAnim != null && currentAnim.Length > 0)
        {
            animFrame = animFrame % currentAnim.Length;
            Sprite spriteToUse = currentAnim[animFrame];

            if (targetImage != null) targetImage.sprite = spriteToUse;
            if (targetSprite != null) targetSprite.sprite = spriteToUse;
        }
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        if (rb != null)
        {
            // Move using Physics
            // Multiply by bigger factor because force/velocity needs it
            rb.linearVelocity = moveInput * moveSpeed * Time.fixedDeltaTime * 10f; 
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish")) // Goal tag
        {
            Debug.Log("[Labyrinth] Reached Goal!");
            LabyrinthGameManager.Instance.WinGame();
        }
        else if (other.CompareTag("Obstacle")) // Hazards (optional)
        {
            Debug.Log("[Labyrinth] Hit Hazard!");
            LabyrinthGameManager.Instance.GameOver();
        }
    }
}
