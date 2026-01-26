using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController25D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f; // New Jump Strength
    public float interactionRange = 1.5f;
    public LayerMask groundLayer; // To know what is "Ground"

    [Header("References")]
    public Transform spriteVisuals; 
    public InputActionReference moveAction; 
    public InputActionReference jumpAction; // New Jump Input

    [Header("Audio")]
    public AudioClip[] footstepSounds;
    public float footstepInterval = 0.5f;

    private Rigidbody rb;
    private Vector2 inputVector;
    private bool isFacingRight = true;
    private bool jumpRequested = false;
    private float footstepTimer;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Ensure Physics is 3D Standard
        rb.freezeRotation = true; 
        rb.useGravity = true; // Gravity is needed for Jumping!

        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += ctx => Jump();
        }
    }

    void Update()
    {
        // 1. Read Input
        if (moveAction != null)
        {
            inputVector = moveAction.action.ReadValue<Vector2>();
        }

        // FOOTSTEPS
        if (inputVector.magnitude > 0.1f && IsGrounded())
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayRandomFootstep(footstepSounds, audioSource);
                }
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0.1f; // Ready to step immediately when moving starts
        }

        // 2. Handle Sprite Flip (Facing Direction)
        if (inputVector.x > 0.1f && !isFacingRight)
        {
            Flip(true);
        }
        else if (inputVector.x < -0.1f && isFacingRight)
        {
            Flip(false);
        }
    }

    void Jump()
    {
        if (IsGrounded())
        {
            jumpRequested = true;
        }
    }

    bool IsGrounded()
    {
        // Simple Raycast down
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }

    void FixedUpdate()
    {
        // 3. Move via Physics (X/Z)
        Vector3 targetVel = new Vector3(inputVector.x * moveSpeed, rb.linearVelocity.y, inputVector.y * moveSpeed);
        rb.linearVelocity = targetVel;

        // 4. Handle Jump (Y)
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    void Flip(bool faceRight)
    {
        isFacingRight = faceRight;
        if (spriteVisuals != null)
        {
            Vector3 scale = spriteVisuals.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            spriteVisuals.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
