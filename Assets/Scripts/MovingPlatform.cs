using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2.0f;
    
    private float t;
    private bool movingToB = true;

    void Update()
    {
        if (pointA == null || pointB == null) return;

        // Move the platform
        float step = speed * Time.deltaTime;
        Transform target = movingToB ? pointB : pointA;
        
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        
        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            movingToB = !movingToB;
        }
    }

    // Stick Player to Platform
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<LabyrinthPlayerController>() != null)
        {
            other.transform.SetParent(transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<LabyrinthPlayerController>() != null)
        {
            other.transform.SetParent(null); // Or reset to original parent if UI
            // Important: For UI, parenting changes positions. 
            // If this is UI Parkour, parenting might break layout. 
            // If World Parkour, parenting is fine.
        }
    }
}
