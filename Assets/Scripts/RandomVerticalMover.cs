using UnityEngine;

public class RandomVerticalMover : MonoBehaviour
{
    public float height = 2f;
    public float speedMin = 1f;
    public float speedMax = 3f;
    
    private Vector3 startPos;
    private float speed;
    private float offset;

    void Start()
    {
        startPos = transform.position;
        speed = Random.Range(speedMin, speedMax);
        offset = Random.Range(0f, 2f * Mathf.PI); // Random start phase
    }

    void Update()
    {
        // Simple Sine Wave movement
        float newY = startPos.y + Mathf.Sin(Time.time * speed + offset) * height;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private Transform originalParent;

    // Support sticking player
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlatformerController2D>())
        {
            if (originalParent == null) originalParent = collision.transform.parent; // Cache it
            collision.transform.SetParent(transform);
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlatformerController2D>())
        {
            // Restore original parent (Canvas) instead of null!
            if (originalParent != null)
            {
                 collision.transform.SetParent(originalParent);
                 originalParent = null;
            }
            else
            {
                 collision.transform.SetParent(null); // Fallback
            }
        }
    }
}
