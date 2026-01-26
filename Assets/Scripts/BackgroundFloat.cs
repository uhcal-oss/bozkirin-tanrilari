using UnityEngine;

public class BackgroundFloat : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float moveAmount = 0.2f; // How far it moves
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Create a smooth wave motion (Left and Right)
        float newX = startPos.x + Mathf.Sin(Time.time * moveSpeed) * moveAmount;
        transform.position = new Vector3(newX, startPos.y, startPos.z);
    }
}