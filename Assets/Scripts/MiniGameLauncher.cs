using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MiniGameLauncher : MonoBehaviour
{
    public static MiniGameLauncher Instance;

    [System.Serializable]
    public class MiniGameEntry
    {
        public string gameName;
        public GameObject miniGamePanel;
        public GameObject gameManagerObject; 
        public AudioClip minigameMusic;
        public UnityEvent onWin; // NEW: Specific event for this game
    }

    [Header("Games List")]
    public List<MiniGameEntry> games = new List<MiniGameEntry>();
    
    [Header("Events")]
    public UnityEvent onMiniGameWin; // Global (optional)

    private MonoBehaviour playerScript;
    private int currentGameIndex = -1;

    [Header("UI References")]
    public GameObject fadePanel; // Assign FadePanel (Black Image)

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Launch a specific minigame by index.
    /// Call this from Dialogue System events (e.g. LaunchGame(0))
    /// </summary>
    public void LaunchGame(int index)
    {
        StartCoroutine(LaunchGameRoutine(index));
    }

    IEnumerator LaunchGameRoutine(int index)
    {
        if (index < 0 || index >= games.Count) yield break;

        Debug.Log($"[MiniGame] Launching Game #{index}: {games[index].gameName}");
        
        // Prevent Re-entry
        if (HeartManager.Instance != null && index < HeartManager.Instance.heartsCollected.Length && HeartManager.Instance.heartsCollected[index])
        {
             Debug.Log($"[MiniGame] Game #{index} already completed! Skipping.");
             yield break;
        }

        currentGameIndex = index;
        MiniGameEntry game = games[index];

        // 1. FREEZE PLAYER
        FreezePlayer();

        // 2. FADE OUT (To Black)
        yield return StartCoroutine(Fade(0f, 1f));

        // 3. ACTIVATE PANEL
        if (game.miniGamePanel != null)
        {
            game.miniGamePanel.SetActive(true);
        }

        // 4. START MUSIC
        if (game.minigameMusic != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(game.minigameMusic);
        }

        // 5. START LOGIC
        if (game.gameManagerObject != null)
        {
            IMiniGame minigame = game.gameManagerObject.GetComponent<IMiniGame>();
            if (minigame != null) minigame.StartGame();
        }

        // 6. FADE IN (To Game)
        yield return StartCoroutine(Fade(1f, 0f));
    }

    void FreezePlayer()
    {
        playerScript = FindFirstObjectByType<PlayerController25D>() as MonoBehaviour;
        if (playerScript == null) playerScript = FindFirstObjectByType<UndertaleMovement>() as MonoBehaviour;
        
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }
    }

    public void WinGame()
    {
        StartCoroutine(WinGameRoutine());
    }

    IEnumerator WinGameRoutine()
    {
         Debug.Log("[MiniGame] WinGame called!");

         // 1. FADE OUT (To Black)
         yield return StartCoroutine(Fade(0f, 1f));
        
        // 2. HIDE PANEL
        if (currentGameIndex >= 0 && currentGameIndex < games.Count)
        {
            if (games[currentGameIndex].miniGamePanel != null)
            {
                games[currentGameIndex].miniGamePanel.SetActive(false);
            }

            // Award Heart!
            if (HeartManager.Instance != null)
            {
                HeartManager.Instance.CollectHeart(currentGameIndex);
            }
            
            // Invoke Specific Game Win Event
            if (games[currentGameIndex].onWin != null)
            {
                games[currentGameIndex].onWin.Invoke();
            }
        }
        
        // 3. STOP MUSIC
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.musicSource.Stop();
        }

        // 4. UNFREEZE PLAYER
        if (playerScript != null)
        {
            playerScript.enabled = true;
        }
        
        // 5. FADE IN
        yield return StartCoroutine(Fade(1f, 0f));

        // 6. GLOBAL EVENT
        if (onMiniGameWin != null)
        {
            onMiniGameWin.Invoke();
        }
    }

    // Helper Fade
    IEnumerator Fade(float start, float end)
    {
        if (fadePanel == null) yield break;
        
        fadePanel.SetActive(true);
        CanvasGroup cg = fadePanel.GetComponent<CanvasGroup>();
        UnityEngine.UI.Image img = fadePanel.GetComponent<UnityEngine.UI.Image>();
        
        float t = 0f;
        float duration = 0.5f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration; // Unscaled in case time stops?
            float a = Mathf.Lerp(start, end, t);
            if (cg != null) cg.alpha = a;
            else if (img != null) 
            {
                Color c = img.color;
                c.a = a;
                img.color = c;
            }
            yield return null;
        }

        if (end <= 0.01f) fadePanel.SetActive(false);
    }
}
