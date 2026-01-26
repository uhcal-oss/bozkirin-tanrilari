using UnityEngine;

public class MusicStarter : MonoBehaviour
{
    [Header("Settings")]
    public AudioClip musicToPlay;
    public float delay = 0.5f; // Small delay to ensure AudioManager is ready

    void Start()
    {
        if (AudioManager.Instance != null && musicToPlay != null)
        {
            // Play immediately (or with slight delay)
            StartCoroutine(StartMusicSequence());
        }
    }

    System.Collections.IEnumerator StartMusicSequence()
    {
        yield return new WaitForSeconds(delay);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(musicToPlay);
            Debug.Log($"[MusicStarter] Playing start music: {musicToPlay.name}");
        }
    }
}
