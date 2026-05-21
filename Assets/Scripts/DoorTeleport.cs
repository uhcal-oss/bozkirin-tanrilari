using UnityEngine;
using System.Collections;

public class DoorTeleport : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Where the player will appear. Create an empty GameObject at the destination.")]
    public Transform destination; 
    public float transitionTime = 1.0f;

    [Header("UI References")]
    [Tooltip("The black panel for fading (Can use the same one as BedSleep)")]
    public CanvasGroup fadeOverlay; 
    
    [Header("Audio")]
    public AudioClip roomMusic; // Music to play when entering this room 

    [Header("Events")]
    [Tooltip("Triggered automatically when the teleport finishes fading in.")]
    public UnityEngine.Events.UnityEvent onTeleportComplete;

    private bool isTeleporting = false;

    /// <summary>
    /// Call this from Interactable.OnInteract
    /// </summary>
    public void ActivateDoor()
    {
        FileLogger.Log($"[DoorTeleport] ActivateDoor called on {name}"); 

        if (isTeleporting) return;
        if (destination == null)
        {
            FileLogger.Log($"[DoorTeleport] Error: No destination assigned on {name}!");
            return;
        }

        StartCoroutine(TeleportSequence());
    }

    IEnumerator TeleportSequence()
    {
        isTeleporting = true;

        // Find Player generically (Not tied to one script)
        Transform playerXform = FindPlayer();
        
        if (playerXform == null)
        {
            FileLogger.Log("[DoorTeleport] Error: Could not find Player object!");
            isTeleporting = false;
            yield break;
        }

        FileLogger.Log($"[DoorTeleport] Teleporting {playerXform.name} to {destination.name}...");

        // Disable Player Movement components if found
        // Only disable known movement scripts to prevent walking during fade
        var p1 = playerXform.GetComponent<PlayerController25D>();
        var p2 = playerXform.GetComponent<UndertaleMovement>(); // Assuming this exists
        if (p1 != null) p1.enabled = false;
        if (p2 != null) p2.enabled = false;

        // FADE OUT
        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            fadeOverlay.alpha = 0;
            float t = 0;
            while (t < 1.0f)
            {
                t += Time.deltaTime / (transitionTime * 0.5f);
                fadeOverlay.alpha = t;
                yield return null;
            }
            fadeOverlay.alpha = 1;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // MOVE PLAYER
        playerXform.position = destination.position;
        FileLogger.Log($"[DoorTeleport] Moved player position.");

        // CHANGE ROOM MUSIC
        if (roomMusic != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(roomMusic);
            Debug.Log($"[DoorTeleport] Changed music to {roomMusic.name}");
        }

        yield return new WaitForSeconds(0.2f); 
        
        // FADE IN
        if (fadeOverlay != null)
        {
            float t = 1.0f;
            while (t > 0.0f)
            {
                t -= Time.deltaTime / (transitionTime * 0.5f);
                fadeOverlay.alpha = t;
                yield return null;
            }
            fadeOverlay.alpha = 0;
            fadeOverlay.blocksRaycasts = false;
        }

        // Enable Player Movement
        if (p1 != null) p1.enabled = true;
        if (p2 != null) p2.enabled = true;

        isTeleporting = false;

        // Trigger completing event (e.g. for automatic dialogue)
        onTeleportComplete?.Invoke();
    }

    Transform FindPlayer()
    {
        // 1. Check Tag
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) return p.transform;

        // 2. Check Name explicitly
        p = GameObject.Find("Yeliz");
        if (p != null) return p.transform;
        
        p = GameObject.Find("Player");
        if (p != null) return p.transform;

        // 3. Check Scripts (Slow but fallback)
        var s1 = FindFirstObjectByType<PlayerController25D>();
        if (s1 != null) return s1.transform;

        var s2 = FindFirstObjectByType<UndertaleMovement>();
        if (s2 != null) return s2.transform;

        return null; // Give up
    }
}
