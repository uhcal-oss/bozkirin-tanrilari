using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class InGameVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoClip videoClip;
    
    [Header("Transition")]
    [Tooltip("If you want to go to another scene after the video (e.g. 'MainMenu'), type it here. Leave empty to just resume the game.")]
    public string nextSceneName = "";

    [Header("Events")]
    public UnityEvent onVideoComplete;

    private GameObject canvasObj;
    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private AspectRatioFitter aspectFitter;

    private bool isPrepared = false;
    private bool isPlaying = false;
    private AudioSource videoAudioSource;

    /// <summary>
    /// Call this from an event (like the Minigame OnWin event)
    /// </summary>
    public void PlayVideo()
    {
        if (isPlaying) return;
        StartCoroutine(VideoSequence());
    }

    private void SetupUI()
    {
        // 1. Create a Canvas to render on top
        canvasObj = new GameObject("InGameVideoCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; 
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // 1b. Black background
        GameObject bgObj = new GameObject("SolidBackground");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.black;
        RectTransform bgRect = bgImage.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // 2. Create RawImage for video
        GameObject rawImageObj = new GameObject("VideoRawImage");
        rawImageObj.transform.SetParent(canvasObj.transform, false);
        
        rawImage = rawImageObj.AddComponent<RawImage>();
        rawImage.color = Color.white;
        
        RectTransform rawRect = rawImage.GetComponent<RectTransform>();
        rawRect.anchorMin = Vector2.zero;
        rawRect.anchorMax = Vector2.one;
        rawRect.sizeDelta = Vector2.zero;
        
        aspectFitter = rawImageObj.AddComponent<AspectRatioFitter>();
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent; 
        aspectFitter.aspectRatio = 16f / 9f;
        
        // 3. Setup VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoAudioSource = gameObject.AddComponent<AudioSource>();
        
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudioSource);
        
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
        }

        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        rawImage.texture = vp.texture;
        aspectFitter.aspectRatio = (float)vp.texture.width / vp.texture.height;
        isPrepared = true;
    }

    private IEnumerator VideoSequence()
    {
        isPlaying = true;

        SetupUI();

        // Pause Game while video plays
        Time.timeScale = 0f;

        videoPlayer.Prepare();

        // Wait to prepare (in real time since timeScale is 0)
        while (!isPrepared)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }

        videoPlayer.Play();

        // Wait for video to finish
        // We use WaitUntil because WaitForSecondsRealtime requires a fixed time
        yield return new WaitUntil(() => videoPlayer.isPlaying == false && videoPlayer.time > 1.0f);

        // Cleanup
        Time.timeScale = 1f;
        
        if (canvasObj != null)
        {
            Destroy(canvasObj);
        }

        if (videoPlayer != null) Destroy(videoPlayer);
        if (videoAudioSource != null) Destroy(videoAudioSource);

        isPlaying = false;
        isPrepared = false;

        onVideoComplete?.Invoke();

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}