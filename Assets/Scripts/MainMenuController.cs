using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // Added this!
using UnityEngine.Video;

public class MainMenuController : MonoBehaviour
{
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    
    // Drag your "Start" button here in the Inspector!
    public GameObject firstSelectedButton; 
    
    [Header("Settings")]
    public float transitionDelay = 0.5f; // Delay for animations to finish
    public int settingsSceneIndex = 2; // Default to build index 2 

    [Header("Intro Video")]
    public VideoClip introVideoClip;
    [Tooltip("Relative path under Assets folder if VideoClip is not set, e.g. Video/Tailer.mp4")]
    public string introVideoPath = "Video/Tailer.mp4";

    void Awake()
    {
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    void Start()
    {
        // 1. Show the mouse cursor and keep it in the window
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined; // Allows movement but keeps it inside game window

        // 2. Tell Unity to select the Start button automatically
        // This makes Arrow Keys work instantly
        if (firstSelectedButton!= null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    private System.Collections.IEnumerator StartGameRoutine()
    {
        if(sfxSource != null && clickSound != null) sfxSource.PlayOneShot(clickSound);
        
        // --- NEW GAME: WIPE PROGRESS ---
        ClearSaveData();

        // Trigger Exit Animations
        foreach (var slide in FindObjectsByType<MenuSlideIn>(FindObjectsSortMode.None))
        {
            slide.PlayExitAnimation();
        }

        // Wait for animations (like cursor flash)
        yield return new WaitForSeconds(transitionDelay);

        // Find or create IntroVideoPlayer
        IntroVideoPlayer videoPlayer = FindAnyObjectByType<IntroVideoPlayer>();
        if (videoPlayer == null)
        {
            GameObject vpObj = new GameObject("IntroVideoPlayerInstance");
            videoPlayer = vpObj.AddComponent<IntroVideoPlayer>();
        }

        // Assign the video settings
        videoPlayer.videoClip = introVideoClip;
        videoPlayer.videoRelativePath = introVideoPath;

        // Play the video and load scene index 1 when done
        videoPlayer.PlayVideoAndLoadScene(1);
    }

    private void ClearSaveData()
    {
        Debug.Log("[MainMenu] Starting New Game. Wiping old save data...");

        // Wipe position and scene save
        PlayerPrefs.DeleteKey("HasSave");
        PlayerPrefs.DeleteKey("SaveX");
        PlayerPrefs.DeleteKey("SaveY");
        PlayerPrefs.DeleteKey("SaveZ");
        PlayerPrefs.DeleteKey("SavedScene");

        // Wipe Daily Activities & Timers
        PlayerPrefs.DeleteKey("DailyInteractions");
        PlayerPrefs.DeleteKey("ClassTimeState");

        // Wipe Hearts
        for (int i = 0; i < 10; i++)
        {
            PlayerPrefs.DeleteKey($"Heart_{i}");
        }

        PlayerPrefs.Save();
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameRoutine());
    }

    private System.Collections.IEnumerator QuitGameRoutine()
    {
        if(sfxSource != null && clickSound != null) sfxSource.PlayOneShot(clickSound);
        
        // Trigger Exit Animations
        foreach (var slide in FindObjectsByType<MenuSlideIn>(FindObjectsSortMode.None))
        {
            slide.PlayExitAnimation();
        }

        yield return new WaitForSeconds(transitionDelay);

        Application.Quit();
        Debug.Log("Game is quitting...");
    }

    public void OpenSettings()
    {
        StartCoroutine(OpenSettingsRoutine());
    }

    private System.Collections.IEnumerator OpenSettingsRoutine()
    {
        if(sfxSource != null && clickSound != null) sfxSource.PlayOneShot(clickSound);
        
        // Trigger Exit Animations
        foreach (var slide in FindObjectsByType<MenuSlideIn>(FindObjectsSortMode.None))
        {
            slide.PlayExitAnimation();
        }

        yield return new WaitForSeconds(transitionDelay);

        SceneManager.LoadScene(settingsSceneIndex);
    }

    public void PlayHoverSound()
    {
        if(sfxSource != null && hoverSound != null) sfxSource.PlayOneShot(hoverSound);
    }
}