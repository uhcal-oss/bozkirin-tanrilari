using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class IntroVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("The Video Clip to play. If null, we will try to load from the streaming path.")]
    public VideoClip videoClip;
    
    [Tooltip("The path of the video relative to the Assets folder (e.g. Video/Tailer.mp4). Used if videoClip is null.")]
    public string videoRelativePath = "Video/Tailer.mp4";

    [Header("Transition Settings")]
    public float fadeInDuration = 1.0f;
    public float fadeOutDuration = 1.0f;

    [Header("Skip Settings")]
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;
    public float showSkipPromptAfter = 2.0f;
    public Font skipPromptFont;

    // Internal UI elements (created programmatically)
    private GameObject canvasObj;
    private CanvasGroup fadeCanvasGroup;
    private VideoPlayer videoPlayer;
    private AudioSource videoAudioSource;
    private GameObject skipPrompt;
    private RawImage rawImage;
    private AspectRatioFitter aspectFitter;

    private bool isPrepared = false;
    private bool isTransitioning = false;

    // Loading target settings
    private int nextSceneIndex = 1;
    private string nextSceneName = "Level1";
    private bool useIndex = true;

    // Structure to remember audio states
    private struct AudioSourceMuteInfo
    {
        public AudioSource source;
        public float originalVolume;
        public bool originalMute;
    }
    private List<AudioSourceMuteInfo> mutedSources = new List<AudioSourceMuteInfo>();

    void Awake()
    {
        // Keep this object alive during transition to ensure scene loads smoothly
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Starts the video playback and loads the target scene index when finished.
    /// </summary>
    public void PlayVideoAndLoadScene(int targetSceneIndex)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        nextSceneIndex = targetSceneIndex;
        useIndex = true;
        
        StartCoroutine(PlaybackSequence());
    }

    /// <summary>
    /// Starts the video playback and loads the target scene name when finished.
    /// </summary>
    public void PlayVideoAndLoadScene(string targetSceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        nextSceneName = targetSceneName;
        useIndex = false;
        
        StartCoroutine(PlaybackSequence());
    }

    private void SetupUI()
    {
        // 1. Create a Canvas GameObject (as a root GameObject for clean DontDestroyOnLoad)
        canvasObj = new GameObject("IntroVideoCanvas");
        DontDestroyOnLoad(canvasObj); // Make sure the Canvas survives scene load
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Render on top of everything
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 1b. Create solid black background panel (to prevent menu/scene peeking through when letterboxed)
        GameObject bgObj = new GameObject("SolidBackground");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.black;
        RectTransform bgRect = bgImage.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // 2. Create RawImage for video rendering
        GameObject rawImageObj = new GameObject("VideoRawImage");
        rawImageObj.transform.SetParent(canvasObj.transform, false);
        
        rawImage = rawImageObj.AddComponent<RawImage>();
        rawImage.color = Color.white;
        
        // Stretch RawImage to cover the full screen
        RectTransform rawRect = rawImage.GetComponent<RectTransform>();
        rawRect.anchorMin = Vector2.zero;
        rawRect.anchorMax = Vector2.one;
        rawRect.sizeDelta = Vector2.zero;
        
        // Add AspectRatioFitter to avoid distortion (Fit inside screen bounds)
        aspectFitter = rawImageObj.AddComponent<AspectRatioFitter>();
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent; 
        aspectFitter.aspectRatio = 16f / 9f; // Default, will update once video loads
        
        // 3. Create Black Screen for fading
        GameObject fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(canvasObj.transform, false);
        
        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = Color.black;
        
        RectTransform fadeRect = fadeImage.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.sizeDelta = Vector2.zero;
        
        fadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 1f; // Start fully black
        
        // 4. Create Skip Prompt (UI text at bottom right)
        if (allowSkip)
        {
            GameObject skipPromptObj = new GameObject("SkipPrompt");
            skipPromptObj.transform.SetParent(canvasObj.transform, false);
            
            Text skipText = skipPromptObj.AddComponent<Text>();
            skipText.text = "Press SPACE to skip";
            
            if (skipPromptFont == null)
            {
                skipPromptFont = Resources.Load<Font>("Fonts/VCR_OSD_MONO_1.001");
                if (skipPromptFont == null)
                {
                    skipPromptFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
            }
            
            skipText.font = skipPromptFont;
            skipText.fontSize = 24;
            skipText.color = new Color(1f, 1f, 1f, 0.6f);
            skipText.alignment = TextAnchor.LowerRight;
            
            RectTransform skipRect = skipPromptObj.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(0.5f, 0f);
            skipRect.anchorMax = new Vector2(1f, 0f);
            skipRect.pivot = new Vector2(1f, 0f);
            skipRect.anchoredPosition = new Vector2(-50f, 50f);
            skipRect.sizeDelta = new Vector2(400f, 100f);
            
            skipPrompt = skipPromptObj;
            skipPrompt.SetActive(false); // Hide initially
        }
        
        // 5. Setup Video Player and Audio Source
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoAudioSource = gameObject.AddComponent<AudioSource>();
        
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudioSource);
        
        // Hook prepared event
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (rawImage != null) rawImage.texture = vp.texture;
        if (aspectFitter != null) aspectFitter.aspectRatio = (float)vp.texture.width / vp.texture.height;
        isPrepared = true;
    }

    private void MuteAllOtherAudioSources()
    {
        mutedSources.Clear();
        // Find all audio sources in the scene
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in sources)
        {
            if (src == null || src == videoAudioSource) continue;
            
            AudioSourceMuteInfo info = new AudioSourceMuteInfo
            {
                source = src,
                originalVolume = src.volume,
                originalMute = src.mute
            };
            mutedSources.Add(info);
            src.mute = true;
        }
    }

    private void RestoreOtherAudioSources()
    {
        foreach (var info in mutedSources)
        {
            if (info.source != null)
            {
                try
                {
                    info.source.mute = info.originalMute;
                    info.source.volume = info.originalVolume;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[IntroVideoPlayer] Could not restore audio source state: {e.Message}");
                }
            }
        }
        mutedSources.Clear();
    }

    private IEnumerator PlaybackSequence()
    {
        Debug.Log("[IntroVideoPlayer] Starting PlaybackSequence coroutine...");
        
        // 1. Programmatically set up the UI and Video Player
        bool setupFailed = false;
        try
        {
            SetupUI();
            Debug.Log("[IntroVideoPlayer] SetupUI completed successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[IntroVideoPlayer] Error in SetupUI: {e}");
            setupFailed = true;
        }

        if (setupFailed)
        {
            yield return StartCoroutine(TransitionToSceneOnly());
            yield break;
        }

        // 2. Load the video clip/URL
        if (videoClip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = videoClip;
            Debug.Log($"[IntroVideoPlayer] Loading video clip: {videoClip.name}");
        }
        else
        {
            videoPlayer.source = VideoSource.Url;
            string fullPath = System.IO.Path.Combine(Application.dataPath, videoRelativePath);
            Debug.Log($"[IntroVideoPlayer] Loading video from URL: {fullPath}");
            
            if (System.IO.File.Exists(fullPath))
            {
                videoPlayer.url = fullPath;
            }
            else
            {
                Debug.LogWarning($"[IntroVideoPlayer] Video file not found at: {fullPath}. Skipping intro.");
                yield return StartCoroutine(TransitionToSceneOnly());
                yield break;
            }
        }

        // 3. Prepare the video
        Debug.Log("[IntroVideoPlayer] Preparing video...");
        videoPlayer.Prepare();
        
        float prepareTimer = 0f;
        while (!isPrepared && prepareTimer < 5.0f)
        {
            prepareTimer += Time.deltaTime;
            yield return null;
        }

        if (!isPrepared)
        {
            Debug.LogError("[IntroVideoPlayer] Video failed to prepare in time. Transitioning directly.");
            yield return StartCoroutine(TransitionToSceneOnly());
            yield break;
        }
        Debug.Log("[IntroVideoPlayer] Video prepared successfully.");

        // 3b. Mute all other audio sources in the scene (prevents clashing sfx/music)
        try
        {
            MuteAllOtherAudioSources();
            Debug.Log("[IntroVideoPlayer] Muted all other audio sources.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[IntroVideoPlayer] Warning muting other audio sources: {e.Message}");
        }

        // 3c. Explicitly stop the main menu music track (prevents it from resuming when we unmute)
        if (AudioManager.Instance != null && AudioManager.Instance.musicSource != null)
        {
            try
            {
                AudioManager.Instance.musicSource.Stop();
                Debug.Log("[IntroVideoPlayer] Stopped AudioManager music source.");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[IntroVideoPlayer] Warning stopping AudioManager music: {e.Message}");
            }
        }

        // 4. Start playing video
        videoPlayer.Play();
        Debug.Log("[IntroVideoPlayer] Video playback started.");
        float originalVolume = videoAudioSource != null ? videoAudioSource.volume : 1f;

        // 5. Fade in from black
        Debug.Log("[IntroVideoPlayer] Fading in video visuals...");
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            if (fadeCanvasGroup != null)
            {
                try { fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeInDuration); } catch {}
            }
            yield return null;
        }
        if (fadeCanvasGroup != null) { try { fadeCanvasGroup.alpha = 0f; } catch {} }
        Debug.Log("[IntroVideoPlayer] Fade in completed.");

        // 6. Monitor playback and skip inputs
        float playbackTime = 0f;
        while (videoPlayer != null && videoPlayer.isPlaying && (videoPlayer.time < videoPlayer.length - 0.1f))
        {
            playbackTime += Time.deltaTime;

            if (allowSkip && skipPrompt != null && !skipPrompt.activeSelf && playbackTime >= showSkipPromptAfter)
            {
                skipPrompt.SetActive(true);
            }

            if (IsSkipPressed())
            {
                Debug.Log("[IntroVideoPlayer] User skipped video playback.");
                break;
            }

            yield return null;
        }
        Debug.Log("[IntroVideoPlayer] Video playback ended.");

        // 7. Fade out to black (visuals and audio)
        Debug.Log("[IntroVideoPlayer] Fading out video visuals and audio...");
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float ratio = timer / fadeOutDuration;
            if (fadeCanvasGroup != null)
            {
                try { fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, ratio); } catch {}
            }
            if (videoAudioSource != null)
            {
                try { videoAudioSource.volume = Mathf.Lerp(originalVolume, 0f, ratio); } catch {}
            }
            yield return null;
        }
        if (fadeCanvasGroup != null) { try { fadeCanvasGroup.alpha = 1f; } catch {} }
        if (videoAudioSource != null) { try { videoAudioSource.volume = 0f; } catch {} }
        
        if (videoPlayer != null) videoPlayer.Stop();
        Debug.Log("[IntroVideoPlayer] Fade out completed.");

        // 8. Load the next scene (using index or name)
        Debug.Log($"[IntroVideoPlayer] Loading scene (useIndex={useIndex}, index={nextSceneIndex}, name={nextSceneName})...");
        AsyncOperation asyncLoad = null;
        try
        {
            if (useIndex)
            {
                asyncLoad = SceneManager.LoadSceneAsync(nextSceneIndex);
            }
            else
            {
                asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[IntroVideoPlayer] Exception during LoadSceneAsync call: {e}");
        }

        if (asyncLoad != null)
        {
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("[IntroVideoPlayer] LoadSceneAsync returned null. Falling back to synchronous LoadScene.");
            try
            {
                if (useIndex) SceneManager.LoadScene(nextSceneIndex);
                else SceneManager.LoadScene(nextSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[IntroVideoPlayer] Exception during fallback synchronous LoadScene: {e}");
            }
            yield return null;
        }
        Debug.Log("[IntroVideoPlayer] Next scene loaded successfully.");

        // 8b. Restore other audio sources (so persistent ones can play in Level1)
        try
        {
            RestoreOtherAudioSources();
            Debug.Log("[IntroVideoPlayer] Restored other audio sources.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[IntroVideoPlayer] Error in RestoreOtherAudioSources: {e}");
        }

        // 9. Fade back in from black in the new scene
        Debug.Log("[IntroVideoPlayer] Fading in new scene visuals...");
        timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            if (fadeCanvasGroup != null)
            {
                try { fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeInDuration); } catch {}
            }
            yield return null;
        }
        Debug.Log("[IntroVideoPlayer] Fade in of new scene completed.");

        CleanUp();
    }

    private IEnumerator TransitionToSceneOnly()
    {
        Debug.Log("[IntroVideoPlayer] Starting TransitionToSceneOnly coroutine...");
        AsyncOperation asyncLoad = null;
        if (useIndex)
        {
            asyncLoad = SceneManager.LoadSceneAsync(nextSceneIndex);
        }
        else
        {
            asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        }

        if (asyncLoad != null)
        {
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        else
        {
            if (useIndex) SceneManager.LoadScene(nextSceneIndex);
            else SceneManager.LoadScene(nextSceneName);
            yield return null;
        }
        Debug.Log("[IntroVideoPlayer] TransitionToSceneOnly loaded scene successfully.");

        CleanUp();
    }

    private bool IsSkipPressed()
    {
        if (!allowSkip) return false;

#if UNITY_INPUT_SYSTEM
        try
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)) return true;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;
        }
        catch (System.Exception)
        {
            // Fallback to old input system if new input system is not initialized or throws
        }
#endif

        try
        {
            if (Input.GetKeyDown(skipKey) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                return true;
            }
        }
        catch (System.InvalidOperationException)
        {
            // Old input system is disabled. Ignore and do not crash.
        }
        catch (System.Exception)
        {
            // Catch-all
        }

        return false;
    }

    private void CleanUp()
    {
        Debug.Log("[IntroVideoPlayer] Cleaning up transition components...");
        try
        {
            RestoreOtherAudioSources();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[IntroVideoPlayer] Error restoring audio sources during cleanup: {e.Message}");
        }

        if (canvasObj != null)
        {
            try
            {
                Destroy(canvasObj);
                Debug.Log("[IntroVideoPlayer] Canvas destroyed.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[IntroVideoPlayer] Error destroying Canvas: {e}");
            }
        }
        
        Debug.Log("[IntroVideoPlayer] Destroying IntroVideoPlayer instance.");
        Destroy(gameObject);
    }
}
