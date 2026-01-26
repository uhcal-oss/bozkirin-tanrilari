using UnityEngine;
using TMPro;
using System.Collections;

public class DecodeTitleAnim : MonoBehaviour
{
    public TMP_Text textComponent;
    
    [Header("Settings")]
    public float revealSpeed = 0.1f;    // Time between locking each letter (Lower = Faster Reveal)
    public float scrambleSpeed = 0.03f; // Time between randomizing the unresolved characters (Lower = Faster Flicker)
    public bool loop = false;           // Should it decode continuously? (Usually false for titles)
    public float loopDelay = 5.0f;

    private string originalText;
    private string randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+-=[]{}|;:<>,.?/";

    void Start()
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
        originalText = textComponent.text; 
        
        StartCoroutine(DecodeRoutine());
    }

    IEnumerator DecodeRoutine()
    {
        while (true) // Loop container (if looping is enabled)
        {
            int totalChars = originalText.Length;
            float timeAccumulator = 0f;
            int lockedIndex = 0;

            // Run until all characters are locked
            while (lockedIndex < totalChars)
            {
                // 1. Lock-in Logic
                timeAccumulator += Time.deltaTime;
                if (timeAccumulator >= revealSpeed)
                {
                    lockedIndex++;
                    timeAccumulator = 0f;
                }

                // 2. Build String
                string displayString = "";

                // Part A: Locked (Correct) Text
                if (lockedIndex > 0)
                {
                    displayString += originalText.Substring(0, Mathf.Min(lockedIndex, totalChars));
                }

                // Part B: Scrambled (Random) Text
                if (lockedIndex < totalChars)
                {
                    int remaining = totalChars - lockedIndex;
                    for (int i = 0; i < remaining; i++)
                    {
                        // Use original text length to keep layout stable, or just random chars?
                        // Let's ensure string length stays constant to avoid jittering layout
                        displayString += randomChars[Random.Range(0, randomChars.Length)];
                    }
                }

                textComponent.text = displayString;

                // Wait for scramble flicker speed
                yield return new WaitForSeconds(scrambleSpeed);
            }

            // Ensure final text is clean
            textComponent.text = originalText;

            if (!loop) break;
            
            // If looping, wait and reset
            yield return new WaitForSeconds(loopDelay);
            textComponent.text = ""; // Clear or keep?
        }
    }
}
