using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCWander : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float waitTime = 3f;
    public Transform[] waypoints;

    [Header("Components")]
    public Animator animator;

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private Rigidbody2D rb;
    private Vector2 lastPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();

        // Check for common setup mistake
        Collider2D c = GetComponent<Collider2D>();
        if (c != null && c.isTrigger)
        {
            Debug.LogWarning($"[NPCWander] Warning: The Collider on {name} is set to 'Is Trigger'. The player will walk through it! Add a second Collider execution order or uncheck Is Trigger on this one for physics.");
        }

        
        // Randomize starting waypoint to avoid syncing
        if (waypoints.Length > 0)
        {
            currentWaypointIndex = Random.Range(0, waypoints.Length);
        }
        
        lastPosition = rb.position;
    }

    void FixedUpdate()
    {
        if (isWaiting || waypoints.Length == 0)
        {
            UpdateAnimation(Vector2.zero);
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        float distance = Vector2.Distance(rb.position, target.position);

        if (distance < 0.2f)
        {
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(WaitAtWaypoint());
        }
        else
        {
            // Move using Velocity for better physics collision
            rb.linearVelocity = direction * moveSpeed;
            UpdateAnimation(direction);
        }
    }

    void UpdateAnimation(Vector2 dir)
    {
        if (animator == null) return;

        bool isMoving = dir.magnitude > 0.01f;
        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("InputX", dir.x);
            animator.SetFloat("InputY", dir.y);
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        UpdateAnimation(Vector2.zero);

        yield return new WaitForSeconds(waitTime);

        // Pick next waypoint (random or sequential - let's do random for natural feel)
        currentWaypointIndex = Random.Range(0, waypoints.Length);
        
        isWaiting = false;
    }
}
