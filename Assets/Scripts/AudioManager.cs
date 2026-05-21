using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Settings")]
    public AudioSource musicSource;
    public float musicFadeSpeed = 1.0f;
    
    [Header("Global Music (Full Game)")]
    public AudioClip globalThemeMusic;
    [Tooltip("If true, plays the Global Theme automatically when the game starts.")]
    public bool playGlobalThemeOnStart = false;
    [Tooltip("If true, ignores room music changes from doors or other scripts so the global theme never stops.")]
    public bool ignoreRoomMusicChanges = false;
    [Tooltip("Type the name of scenes where the AudioManager should destroy itself (like MainMenu) to stop the music.")]
    public string[] destroyOnScenes = { "MainMenu" };

    [Header("SFX Settings")]
    public AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        // Auto-create sources if missing
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;

        if (playGlobalThemeOnStart && globalThemeMusic != null)
        {
            musicSource.clip = globalThemeMusic;
            musicSource.volume = 0.5f; // Set to target volume instantly on startup
            musicSource.Play();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (destroyOnScenes != null)
        {
            foreach (string stopScene in destroyOnScenes)
            {
                if (scene.name == stopScene)
                {
                    Debug.Log($"[AudioManager] Reached {stopScene}. Destroying Global Audio.");
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }

    public void PlayMusic(AudioClip newClip)
    {
        if (ignoreRoomMusicChanges) return; // Block track changes if we locked the global theme!
        
        if (newClip == null) return;
        if (musicSource.clip == newClip) return; // Already playing this track

        StartCoroutine(CrossFadeMusic(newClip));
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayRandomFootstep(AudioClip[] clips, AudioSource source, float volume = 0.5f)
    {
        if (clips == null || clips.Length == 0) return;
        if (source == null) return;

        int index = Random.Range(0, clips.Length);
        
        // Slight pitch variation for realism
        source.pitch = Random.Range(0.9f, 1.1f);
        source.PlayOneShot(clips[index], volume);
    }

    System.Collections.IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        // Fade Out
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime * musicFadeSpeed;
                yield return null;
            }
            musicSource.Stop();
        }

        // Switch and Fade In
        musicSource.clip = newClip;
        musicSource.Play();
        
        float targetVolume = 0.5f; // Max music volume
        while (musicSource.volume < targetVolume)
        {
            musicSource.volume += targetVolume * Time.deltaTime * musicFadeSpeed;
            yield return null;
        }
    }
}
