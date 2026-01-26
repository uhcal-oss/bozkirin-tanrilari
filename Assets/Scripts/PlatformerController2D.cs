using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Visuals")]
    // Support both World Sprite and UI Image
    public SpriteRenderer spriteRenderer; 
    public UnityEngine.UI.Image uiImage; 

    // Explicit Sprite Arrays Request
    public Sprite[] idleSprites; // Assuming Idle is Neutral or defaulting to Right?
    public Sprite[] moveRightSprites; // NEW
    public Sprite[] moveLeftSprites;  // NEW
    public Sprite jumpSprite;
    public float animationSpeed = 0.1f;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip[] footstepSounds;
    public float footstepInterval = 0.4f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool jumpRequested;
    private bool isGrounded;
    private bool isFacingRight = true; // Still track facing for Idle/Jump if needed
    
    // Animation
    private float animTimer;
    private int animFrame;

    private AudioSource audioSource;
    private float footstepTimer;
    private bool canMove = true; // Default true for testing

    void Awake()
    {
        Debug.Log($"[Parkour] Awake! Player: {gameObject.name}");
        rb = GetComponent<Rigidbody2D>();
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Setup Physics for Platformer
        rb.gravityScale = 2f; 
        rb.freezeRotation = true;
        
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (uiImage == null) uiImage = GetComponent<UnityEngine.UI.Image>();
        
        if (rb == null) Debug.LogError("[Parkour] Rigidbody2D missing!");
        if (uiImage == null && spriteRenderer == null) Debug.LogError("[Parkour] No Renderer (Sprite/Image) found!");
    }

    public void SetCanMove(bool val)
    {
        Debug.Log($"[Parkour] SetCanMove: {val}");
        canMove = val;
        if (!val && rb != null) rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (!canMove) return;

        // Input
        moveInput = 0;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1;
            
            // Allow Jump if grounded
            if (isGrounded && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame))
            {
                Debug.Log("[Parkour] Jump Input Detected!");
                jumpRequested = true;
            }
        }

        HandleAnimation();
        HandleAudio();
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        // Ground Check
        bool wasGrounded = isGrounded;
        isGrounded = false;
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject) // Ignore self
            {
                isGrounded = true; 
                break;
            }
        }

        
        if (isGrounded != wasGrounded) Debug.Log($"[Parkour] Grounded Changed: {isGrounded}. Radius: {groundCheckRadius}. Layer: {groundLayer.value}. Pos: {transform.position}");

        // Move
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Jump
        if (jumpRequested)
        {
            Debug.Log("[Parkour] Executing Jump Force!");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
            
            if (AudioManager.Instance != null && jumpSound != null)
            {
                AudioManager.Instance.PlaySFX(jumpSound);
            }
        }
        
        // Debug Physics State intermittently
        if (Time.frameCount % 120 == 0) 
        {
           Debug.Log($"[ParkourStatus] Vel: {rb.linearVelocity}, Pos: {transform.position}, Grounded: {isGrounded}");
        }
    }

    void HandleAnimation()
    {
        Sprite spriteToUse = null;

        // Determine State
        if (!isGrounded)
        {
            // JUMP
            if (jumpSprite != null) spriteToUse = jumpSprite; // Could add JumpLeft/Right if requested later
        }
        else
        {
            // GROUNDED
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                // MOVING
                animTimer += Time.deltaTime;
                if (animTimer >= animationSpeed)
                {
                    animTimer = 0;
                    animFrame++;
                }

                if (moveInput > 0)
                {
                    isFacingRight = true;
                    if (moveRightSprites != null && moveRightSprites.Length > 0)
                        spriteToUse = moveRightSprites[animFrame % moveRightSprites.Length];
                }
                else
                {
                    isFacingRight = false;
                    if (moveLeftSprites != null && moveLeftSprites.Length > 0)
                        spriteToUse = moveLeftSprites[animFrame % moveLeftSprites.Length];
                }
            }
            else
            {
                // IDLE
                animFrame = 0;
                if (idleSprites != null && idleSprites.Length > 0)
                    spriteToUse = idleSprites[0];
            }
        }

        // Apply to Component
        if (spriteToUse != null)
        {
            if (spriteRenderer != null) spriteRenderer.sprite = spriteToUse;
            if (uiImage != null) uiImage.sprite = spriteToUse;
        }
    }

    void HandleAudio()
    {
        if (isGrounded && Mathf.Abs(moveInput) > 0.1f)
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
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    [Header("Game Manager")]
    public LabyrinthGameManager assignedManager; // Drag ParkurManager here!

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            Debug.Log("[Parkour] Reached Goal!");
            
            // Priority: Assigned Manager -> Static Instance
            if (assignedManager != null) 
            {
                assignedManager.WinGame();
            }
            else
            {
                LabyrinthGameManager.Instance?.WinGame();
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
            Debug.Log("[Parkour] Died!");
            LabyrinthGameManager.Instance?.GameOver();
        }
    }
}
