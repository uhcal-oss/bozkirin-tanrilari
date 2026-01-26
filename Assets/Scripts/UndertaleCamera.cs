using UnityEngine;

public class UndertaleCamera : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10); // Standard 2D Offset

    void LateUpdate()
    {
        if (target == null) return;

        // Follow the target's position with offset
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // Keep Z fixed at offset.z usually, but Lerp is fine if Z matches.
        smoothedPosition.z = offset.z; 
        
        transform.position = smoothedPosition;
        
        // No LookAt needed for 2D Orthographic
    }
}
