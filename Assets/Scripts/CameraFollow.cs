using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 5, -5); // Default high angle
    public bool lookAtTarget = true;

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f; // Adjust zoom level
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        if (lookAtTarget)
        {
            // In Ortho 2.5D, looking straight or fixed angle is common.
            // transform.LookAt(target); 
            // Usually we want a fixed rotation for Undertale style (e.g. 45 deg down)
            // But let's keep LookAt if user wants to rotate camera
            transform.LookAt(target);
        }
    }
}
