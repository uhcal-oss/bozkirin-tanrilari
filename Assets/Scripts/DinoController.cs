using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM

public class DinoController : MonoBehaviour
{
    [Header("Movement")]
    public float jumpForce = 400f; // Increased for better jump
    public float gravity = -1500f; // MUCH stronger gravity (was -20)
    public float groundY = 0f; // Ground position in local space
    
    [Header("Sprites")]
    public Sprite runSprite1;
    public Sprite runSprite2;
    public Sprite jumpSprite;
    public Sprite crouchSprite;
    public float runAnimSpeed = 0.1f;

    [Header("Colliders")]
    public Vector2 normalColliderSize = new Vector2(70, 70); // Increased from 50x50
    public Vector2 crouchColliderSize = new Vector2(70, 35);

    private Image image;
    private BoxCollider2D col;
    private float verticalVelocity = 0f;
    private bool isGrounded = true;
    private bool isCrouching = false;
    private float runAnimTimer = 0f;
    private bool useRunSprite1 = true;
    private float startXPosition; // Lock horizontal position

    void Start()
    {
        image = GetComponent<Image>();
        col = GetComponent<BoxCollider2D>();
        
        // Ensure collider exists
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            Debug.LogWarning("[Dino] Added missing BoxCollider2D!");
        }
        
        // CRITICAL: Must be trigger for OnTriggerEnter2D
        col.isTrigger = true;
        col.size = normalColliderSize;
        
        // Lock horizontal position
        startXPosition = transform.localPosition.x;
        
        // Fix: Force physics values in case Inspector has old values
        if (gravity > -100f) { gravity = -2000f; Debug.Log("[Dino] Fixed gravity to -2000"); }
        if (jumpForce < 600f) { jumpForce = 800f; Debug.Log("[Dino] Fixed jumpForce to 800"); }

        // Auto-detect ground level from where the Dino is placed in the Editor
        groundY = transform.localPosition.y;
        
        // Add Rigidbody2D if missing (needed for collisions trigger events)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // Kinematic so it doesn't fall by Unity physics
            Debug.Log("[Dino] Added missing Rigidbody2D (Kinematic) for collisions");
        }
        
        Debug.Log($"[Dino] Initialized! Collider size: {col.size}, Is Trigger: {col.isTrigger}");
        Debug.Log($"[Dino] Auto-detected Ground Y level: {groundY}. Gravity: {gravity}, JumpForce: {jumpForce}");
    }

    void Update()
    {
        if (DinoGameManager.Instance != null && DinoGameManager.Instance.IsGameOver())
            return;

        HandleInput();
        ApplyGravity();
        Animate();
        
        // Lock horizontal position (Chrome Dino style - no left/right movement)
        Vector3 pos = transform.localPosition;
        pos.x = startXPosition;
        transform.localPosition = pos;
    }

    void HandleInput()
    {
        if (Keyboard.current == null) return;

        // Jump
        if ((Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) && isGrounded && !isCrouching)
        {
            Jump();
        }

        // Crouch
        if (Keyboard.current.downArrowKey.isPressed)
        {
            if (!isCrouching && isGrounded)
            {
                Crouch();
            }
        }
        else
        {
            if (isCrouching)
            {
                StandUp();
            }
        }
    }

    void Jump()
    {
        verticalVelocity = jumpForce;
        isGrounded = false;
        
        if (DinoGameManager.Instance != null)
        {
            DinoGameManager.Instance.PlaySound(DinoGameManager.Instance.jumpSound);
        }
        
        Debug.Log("[Dino] Jump!");
    }

    void Crouch()
    {
        isCrouching = true;
        col.size = crouchColliderSize;
    }

    void StandUp()
    {
        isCrouching = false;
        col.size = normalColliderSize;
    }

    void ApplyGravity()
    {
        // Apply gravity if we are in the air OR if we are somehow above the ground level (safety check)
        if (!isGrounded || transform.localPosition.y > groundY + 1f)
        {
            verticalVelocity += gravity * Time.deltaTime;
            
            Vector3 pos = transform.localPosition;
            pos.y += verticalVelocity * Time.deltaTime;
            transform.localPosition = pos;

            // Check if landed (using groundY variable)
            if (pos.y <= groundY && verticalVelocity <= 0) // Only land if falling down
            {
                pos.y = groundY;
                transform.localPosition = pos;
                verticalVelocity = 0;
                isGrounded = true;
                Debug.Log("[Dino] Landed!");
            }
        }
    }

    [Header("Audio")]
    public AudioClip[] footstepSounds;

    void Animate()
    {
        if (image == null) return;

        if (!isGrounded)
        {
            // Jumping
            image.sprite = jumpSprite;
        }
        else if (isCrouching)
        {
            // Crouching
            image.sprite = crouchSprite;
        }
        else
        {
            // Running (alternate between 2 sprites)
            runAnimTimer += Time.deltaTime;
            if (runAnimTimer >= runAnimSpeed)
            {
                runAnimTimer = 0;
                useRunSprite1 = !useRunSprite1;
                image.sprite = useRunSprite1 ? runSprite1 : runSprite2;
                
                // Play footstep on sprite change
                if (AudioManager.Instance != null && footstepSounds != null && footstepSounds.Length > 0)
                {
                    AudioManager.Instance.PlayRandomFootstep(footstepSounds, DinoGameManager.Instance.GetComponent<AudioSource>());
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Dino] OnTriggerEnter2D with: {other.gameObject.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("[Dino] Hit obstacle!");
            if (DinoGameManager.Instance != null)
            {
                DinoGameManager.Instance.GameOver();
            }
        }
        else if (other.CompareTag("Collectible"))
        {
            Debug.Log("[Dino] Collected item!");
            if (DinoGameManager.Instance != null)
            {
                DinoGameManager.Instance.CollectItem();
            }
            Destroy(other.gameObject);
        }
    }
}
