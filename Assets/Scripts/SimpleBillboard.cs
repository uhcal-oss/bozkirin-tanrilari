using UnityEngine;

[ExecuteAlways]
public class SimpleBillboard : MonoBehaviour
{
    [Header("Settings")]
    public bool lockXAxis = true; // Essential for "Paper" characters standing upright

    private Camera mainCam;

    void OnEnable()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        // Simply take the camera's rotation
        transform.rotation = mainCam.transform.rotation;

        // If we want them to stand upright (vertical Y), we zero out the X rotation
        if (lockXAxis)
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = 0f; 
            euler.z = 0f; // Prevent weird tilting
            transform.rotation = Quaternion.Euler(euler);
        }
    }
}
