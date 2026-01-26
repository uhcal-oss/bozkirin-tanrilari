using UnityEngine;

public class DinoObstacle : MonoBehaviour
{
    public float moveSpeed = 150f; // Pixels per second (reduced from 300)
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Ensure it has a collider
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            Debug.LogWarning($"[DinoObstacle] Added missing BoxCollider2D to {gameObject.name}!");
        }
        col.isTrigger = true;
        
        // Ensure it has a Rigidbody2D (needed for collisions)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Debug.Log($"[DinoObstacle] {gameObject.name} ready! Tag: {gameObject.tag}, Trigger: {col.isTrigger}");
    }

    void Update()
    {
        if (DinoGameManager.Instance != null && DinoGameManager.Instance.IsGameOver())
            return;

        // Move left
        float speed = DinoGameManager.Instance != null ? DinoGameManager.Instance.GetCurrentSpeed() : moveSpeed;
        rectTransform.anchoredPosition += Vector2.left * speed * Time.deltaTime * 100f;

        // Destroy if off-screen
        if (rectTransform.anchoredPosition.x < -1000)
        {
            Destroy(gameObject);
        }
    }
}
