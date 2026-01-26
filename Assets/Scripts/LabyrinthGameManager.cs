using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class LabyrinthGameManager : MonoBehaviour, IMiniGame
{
    public static LabyrinthGameManager Instance;

    [Header("Setup")]
    public LabyrinthPlayerController playerController;
    public PlatformerController2D platformerController; // NEW: Support for Side scroller
    public Transform startPoint;
    
    [Header("UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI statusText;

    [Header("Audio")]
    public AudioClip winSound;
    public AudioClip loseSound;

    private bool isGameActive = false;
    private AudioSource audioSource;

    [Header("Camera")]
    public float mazeCameraSize = 3f; // Close up for maze
    private float originalCameraSize;
    private Camera mainCamera;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); 
        
        audioSource = gameObject.AddComponent<AudioSource>();
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void StartGame()
    {
        Debug.Log("[Labyrinth] StartGame called!");
        isGameActive = true;
        
        // Setup Camera
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraSize = mainCamera.orthographicSize;
            mainCamera.orthographicSize = mazeCameraSize;
            
            // Set camera target
            UndertaleCamera camScript = mainCamera.GetComponent<UndertaleCamera>();
            if (camScript != null)
            {
                if (playerController != null) camScript.target = playerController.transform;
                else if (platformerController != null) camScript.target = platformerController.transform;
            }
        }
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        // MAZE PLAYER
        if (playerController != null)
        {
            playerController.SetCanMove(true);
            ResetPlayerParent(playerController.transform);
            if (startPoint != null) playerController.transform.position = startPoint.position;
        }

        // PLATFORMER PLAYER
        if (platformerController != null)
        {
            platformerController.SetCanMove(true);
            ResetPlayerParent(platformerController.transform);
            if (startPoint != null) platformerController.transform.position = startPoint.position;
        }
    }

    void ResetPlayerParent(Transform player)
    {
        // CRITICAL: Unparent player just in case they were on a moving platform
        if (player.parent != null && player.parent.GetComponent<MovingPlatform>() != null)
        {
             if (startPoint != null) player.SetParent(startPoint.parent);
             else player.SetParent(null); // World space as fallback
        }
    }

    public void WinGame()
    {
        if (!isGameActive) return;
        isGameActive = false;
        
        Debug.Log("[Labyrinth] YOU WIN!");
        
        // Reset Camera
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = originalCameraSize;
            
            // Reset target? We might leave that to the MiniGameLauncher or next interaction
            // But let's verify if we need to reset it to the main player
            // Usually the main game restore logic handles finding the player
        }

        if (playerController != null) playerController.SetCanMove(false);
        if (platformerController != null) platformerController.SetCanMove(false);

        // Play Sound
        if (AudioManager.Instance != null && winSound != null)
        {
            AudioManager.Instance.PlaySFX(winSound);
        }

        if (statusText != null) statusText.text = "GOAL REACHED!";
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Invoke(nameof(TriggerWinCallback), 1.5f);
    }

    void TriggerWinCallback()
    {
        if (MiniGameLauncher.Instance != null)
        {
            MiniGameLauncher.Instance.WinGame();
        }
        
        // Safety: ensure camera is reset if WinGame didn't fully handle it (or if scene change happens)
        if (mainCamera != null) mainCamera.orthographicSize = originalCameraSize;
    }

    public void GameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;
        
        Debug.Log("[Labyrinth] GAME OVER!");
        
        // Disable Players
        if (playerController != null) playerController.SetCanMove(false);
        if (platformerController != null) platformerController.SetCanMove(false);

        // Sound
        if (AudioManager.Instance != null && loseSound != null)
        {
            AudioManager.Instance.PlaySFX(loseSound);
        }
        
        // Show UI
        if (statusText != null) statusText.text = "YOU DIED\nPress 'R' to Restart";
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    void Update()
    {
        // Restart (Allowed if Active OR if GameOver Panel is showing)
        bool canRestart = isGameActive || (gameOverPanel != null && gameOverPanel.activeSelf);

        if (canRestart && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            StartGame();
        }
    }
}
