using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class UndertaleMovement : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public InputActionReference moveAction;

    [Header("Visuals")]
    public Transform spriteTransform;
    public SpriteRenderer spriteRenderer; // Assign this!

    [Header("Animations")]
    public float frameRate = 0.15f;
    public Sprite[] downSprites; 
    public Sprite[] upSprites;   
    public Sprite[] sideSprites; // Right (or Side)
    public Sprite[] leftSprites; // Optional: Explicit Left

    private Rigidbody2D rb;
    private Vector2 moveInput;
    
    // Animation State
    private float timer;
    private int currentFrame;
    private enum Direction { Down, Up, Right, Left }
    private Direction currentDir = Direction.Down;

    [Header("Audio")]
    public AudioClip[] footstepSounds;
    public float footstepInterval = 0.4f;
    
    private float footstepTimer;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = gameObject.AddComponent<AudioSource>();
        
        rb.gravityScale = 0; 
        rb.freezeRotation = true;
        
        // FIX LAG: Enable Interpolation so movement looks smooth between physics updates
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        // AUTO-SETUP
        if (spriteTransform == null) spriteTransform = transform;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (moveAction != null) moveAction.action.Enable();
    }

    void Update()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        HandleAnimation();
        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        if (moveInput.sqrMagnitude > 0.01f)
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
            footstepTimer = 0.1f; // Ready to step immediately
        }
    }

    void HandleAnimation()
    {
        if (spriteRenderer == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            // Always prioritize horizontal animations since up/down might be missing
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                currentDir = (moveInput.x > 0) ? Direction.Right : Direction.Left;
            }
            else if (Mathf.Abs(moveInput.y) > 0.01f)
            {
                // Only moving vertically
                bool hasUp = upSprites != null && upSprites.Length > 0;
                bool hasDown = downSprites != null && downSprites.Length > 0;

                if (moveInput.y > 0 && hasUp)
                {
                    currentDir = Direction.Up;
                }
                else if (moveInput.y < 0 && hasDown)
                {
                    currentDir = Direction.Down;
                }
                // If moving purely vertical but missing up/down sprites,
                // just keep the current direction (Left or Right).
                // If the game just started and direction was Up/Down initially, default to Right.
                else if (currentDir == Direction.Up || currentDir == Direction.Down)
                {
                    currentDir = Direction.Right;
                }
            }
        }

        // Select Array
        Sprite[] currentAnim = downSprites;
        bool flipX = false;

        switch (currentDir)
        {
            case Direction.Up: currentAnim = upSprites; break;
            case Direction.Down: currentAnim = downSprites; break;
            case Direction.Right: currentAnim = sideSprites; flipX = false; break;
            case Direction.Left: 
                // Use Left array if available, otherwise flip Right
                if (leftSprites != null && leftSprites.Length > 0) 
                {
                    currentAnim = leftSprites; 
                    flipX = false; 
                }
                else 
                {
                    currentAnim = sideSprites; 
                    flipX = true; 
                }
                break;
        }

        // Apply
        spriteRenderer.flipX = flipX;

        // Cycle Frames
        if (isMoving)
        {
             if (currentAnim != null && currentAnim.Length > 0)
             {
                timer += Time.deltaTime;
                if (timer >= frameRate)
                {
                    timer = 0;
                    currentFrame = (currentFrame + 1) % currentAnim.Length;
                }
                
                // CRASH FIX: Wrap index in case array size changed (e.g. Side -> Up)
                currentFrame = currentFrame % currentAnim.Length;
                
                spriteRenderer.sprite = currentAnim[currentFrame];
             }
        }
        else
        {
            // Idle
            timer = 0;
            currentFrame = 0;
            if (currentAnim != null && currentAnim.Length > 0)
                spriteRenderer.sprite = currentAnim[0];
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
