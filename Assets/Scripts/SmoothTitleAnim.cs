using UnityEngine;
using TMPro;

public class SmoothTitleAnim : MonoBehaviour
{
    public TMP_Text textComponent;
    
    [Header("Wave Settings")]
    public float angleMultiplier = 2.0f; // Controls "Waviness" (Higher = more ripples)
    public float speedMultiplier = 2.0f; // Controls Speed
    public float curveScale = 5.0f;      // Controls Height (Amplitude)

    void Awake()
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (textComponent == null) return;

        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // Calculate Sine Wave offset based on Time and Index
            // Each character gets a different phase of the sine wave
            for (int k = 0; k < 4; k++)
            {
                var orig = verts[charInfo.vertexIndex + k];
                
                // Sin(Time * Speed + X_Pos * Angle)
                // Using orig.x ensures the wave flows across the text horizontally
                float offset = Mathf.Sin(Time.time * speedMultiplier + orig.x * 0.01f * angleMultiplier) * curveScale;
                
                verts[charInfo.vertexIndex + k] = orig + new Vector3(0, offset, 0);
            }
        }

        // Apply changes
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
