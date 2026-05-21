using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class InteractableNPCMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public Transform targetLocation; // Where the NPC should walk to

    [Header("Animations (Sprites)")]
    public float frameRate = 0.15f;
    public Sprite[] downSprites; 
    public Sprite[] upSprites;   
    public Sprite[] sideSprites; // Right (or Side)
    public Sprite[] leftSprites; // Optional: Explicit Left

    [Header("Events")]
    public UnityEvent onDestinationReached;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool isMoving = false;
    
    // Animation State
    private float timer;
    private int currentFrame;
    private enum Direction { Down, Up, Right, Left }
    private Direction currentDir = Direction.Down;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.gravityScale = 0; 
            rb.freezeRotation = true;
        }

        // Set idle sprite
        if (downSprites != null && downSprites.Length > 0)
            spriteRenderer.sprite = downSprites[0];
    }

    /// <summary>
    /// Call this from your Interactable's UnityEvent to make the character walk!
    /// </summary>
    public void StartWalking()
    {
        if (targetLocation != null && !isMoving)
        {
            // Lock interaction so we can't talk to them while they walk or after they leave
            Interactable interactable = GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.IsLocked = true;
            }

            // Temporarily disable collision so they can walk through the player
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            StartCoroutine(WalkToTarget());
        }
    }

    private IEnumerator WalkToTarget()
    {
        isMoving = true;

        while (Vector2.Distance(transform.position, targetLocation.position) > 0.1f)
        {
            Vector2 direction = ((Vector2)targetLocation.position - (Vector2)transform.position).normalized;
            
            // Move
            if (rb != null)
            {
                rb.linearVelocity = direction * moveSpeed;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, targetLocation.position, moveSpeed * Time.deltaTime);
            }

            // Animate
            HandleAnimation(direction);

            yield return null; // wait for next frame
        }

        // Destination Reached
        isMoving = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Reset to idle frame mapping to current direction
        HandleAnimation(Vector2.zero);

        Debug.Log($"[InteractableNPCMovement] {gameObject.name} reached destination.");
        onDestinationReached?.Invoke();
    }

    private void HandleAnimation(Vector2 moveInput)
    {
        bool movingNow = moveInput.sqrMagnitude > 0.01f;

        if (movingNow)
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                currentDir = (moveInput.x > 0) ? Direction.Right : Direction.Left;
            }
            else
            {
                currentDir = (moveInput.y > 0) ? Direction.Up : Direction.Down;
            }
        }

        Sprite[] currentAnim = downSprites;
        bool flipX = false;

        switch (currentDir)
        {
            case Direction.Up: 
                currentAnim = upSprites != null && upSprites.Length > 0 ? upSprites : sideSprites; 
                break;
            case Direction.Down: 
                currentAnim = downSprites != null && downSprites.Length > 0 ? downSprites : sideSprites; 
                break;
            case Direction.Right: 
                currentAnim = sideSprites; 
                flipX = false; 
                break;
            case Direction.Left: 
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

        spriteRenderer.flipX = flipX;

        if (movingNow)
        {
             if (currentAnim != null && currentAnim.Length > 0)
             {
                timer += Time.deltaTime;
                if (timer >= frameRate)
                {
                    timer = 0;
                    currentFrame = (currentFrame + 1) % currentAnim.Length;
                }
                currentFrame = currentFrame % currentAnim.Length;
                spriteRenderer.sprite = currentAnim[currentFrame];
             }
        }
        else
        {
            timer = 0;
            currentFrame = 0;
            if (currentAnim != null && currentAnim.Length > 0)
                spriteRenderer.sprite = currentAnim[0];
        }
    }
}