using UnityEngine;

[ExecuteAlways]
public class Billboard : MonoBehaviour
{
    public bool freezeX = false;
    public bool freezeY = false;
    public bool freezeZ = false;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // Get the rotation needed to face the camera
        Quaternion targetRotation = mainCamera.transform.rotation;
        
        // If we want to freeze an axis, we convert to Euler and back?
        // Actually simpler: Billboard usually just means "Paste Rotation".
        // BUT for Undertale style, we usually want Y-Axis freedom if it's billboarding upwards,
        // or just strict copy if looked at from above.
        
        transform.rotation = targetRotation;
        
        // If specific axis locking is needed:
        Vector3 euler = transform.rotation.eulerAngles;
        if (freezeX) euler.x = 0;
        if (freezeY) euler.y = 0; // Don't turn L/R? Rare.
        if (freezeZ) euler.z = 0;
        
        transform.rotation = Quaternion.Euler(euler);
    }
}
