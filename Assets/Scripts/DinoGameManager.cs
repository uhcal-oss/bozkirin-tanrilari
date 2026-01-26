using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM

public class DinoGameManager : MonoBehaviour, IMiniGame
{
    public static DinoGameManager Instance;

    [Header("Game Settings")]
    public int totalCollectibles = 5; // How many items to collect to win
    public float gameSpeed = 2f; // Reduced from 5 for playability
    public float speedIncreaseRate = 0.05f; // Reduced from 0.1

    [Header("UI")]
    public TextMeshProUGUI collectiblesText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip collectSound;
    public AudioClip hitSound;
    public AudioClip winSound;

    private AudioSource audioSource;
    private int collectiblesCollected = 0;
    private bool isGameOver = false;
    private float currentSpeed;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Ensure UI is hidden on load
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void StartGame()
    {
        Debug.Log("[DinoGame] Game Started!");
        collectiblesCollected = 0;
        isGameOver = false;
        currentSpeed = gameSpeed;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateUI();
        
        Time.timeScale = 1f; // Ensure game is running
    }

    public void CollectItem()
    {
        collectiblesCollected++;
        PlaySound(collectSound);
        UpdateUI();

        Debug.Log($"[DinoGame] Collected {collectiblesCollected}/{totalCollectibles}");

        if (collectiblesCollected >= totalCollectibles)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        Debug.Log("[DinoGame] YOU WIN!");
        isGameOver = true;
        PlaySound(winSound);
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null) gameOverText.text = "YOU WON!";
        }

        // Wait a moment then trigger the next dialogue
        Invoke(nameof(TriggerWinCallback), 2f);
    }

    void TriggerWinCallback()
    {
        if (MiniGameLauncher.Instance != null)
        {
            MiniGameLauncher.Instance.WinGame();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        
        Debug.Log("[DinoGame] GAME OVER!");
        isGameOver = true;
        PlaySound(hitSound);
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null) gameOverText.text = "GAME OVER\nPress R to Restart";
        }
    }

    public void RestartGame()
    {
        StartGame();
    }

    void UpdateUI()
    {
        if (collectiblesText != null)
        {
            collectiblesText.text = $"Collected: {collectiblesCollected}/{totalCollectibles}";
        }
    }

    void Update()
    {
        if (isGameOver && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }

        // Gradually increase speed
        if (!isGameOver)
        {
            currentSpeed += speedIncreaseRate * Time.deltaTime;
        }
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
