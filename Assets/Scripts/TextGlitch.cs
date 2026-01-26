using UnityEngine;
using TMPro;

public class TextGlitch : MonoBehaviour
{
    public TMP_Text textComponent;
    public float shakeAmount = 2.0f;

    void Awake()
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (textComponent == null) return;

        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;

        // Loop through every letter
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            // Create a random shake for this frame
            Vector3 jitter = new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), 0);

            // Move the 4 corners of the letter
            int materialIndex = charInfo.materialReferenceIndex;
            var verts = textInfo.meshInfo[materialIndex].vertices;
            for (int j = 0; j < 4; j++)
            {
                verts[charInfo.vertexIndex + j] += jitter;
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