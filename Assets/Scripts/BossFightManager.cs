using UnityEngine;
using System.Collections;

/// <summary>
/// Boss fight minigame. Enables the sword attack + dot cursor on the player
/// when the fight starts, and disables them when the boss is defeated.
/// Implements IMiniGame so it can be launched via MiniGameLauncher.
/// </summary>
public class BossFightManager : MonoBehaviour, IMiniGame
{
    public static BossFightManager Instance;

    [Header("Boss")]
    [Tooltip("The boss GameObject (needs a Health/TakeDamage receiver).")]
    public GameObject bossObject;

    [Tooltip("Boss max HP.")]
    public int bossHP = 10;

    [Header("Player References")]
    [Tooltip("The player GameObject (Rua). SwordAttack & GameplayCursor will be enabled on it.")]
    public GameObject playerObject;

    [Header("UI (Optional)")]
    [Tooltip("A UI element showing boss HP (e.g. a Slider or bar).")]
    public UnityEngine.UI.Slider bossHPBar;

    [Header("Settings")]
    [Tooltip("Time before the fight actually starts (intro delay).")]
    public float introDelay = 1.0f;

    private int currentHP;
    private bool fightActive = false;
    private SwordAttack swordAttack;
    private GameplayCursor gameplayCursor;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartGame()
    {
        Debug.Log("[BossFight] Starting boss fight!");
        currentHP = bossHP;
        fightActive = true;

        // Find and enable sword + cursor on player
        if (playerObject == null)
        {
            // Try to find the player
            playerObject = GameObject.FindWithTag("Player");
            if (playerObject == null) playerObject = GameObject.Find("Rua");
            if (playerObject == null) playerObject = GameObject.Find("Yeliz");
        }

        if (playerObject != null)
        {
            // Enable SwordAttack
            swordAttack = playerObject.GetComponent<SwordAttack>();
            if (swordAttack == null)
                swordAttack = playerObject.AddComponent<SwordAttack>();
            swordAttack.enabled = true;

            // Enable GameplayCursor
            gameplayCursor = playerObject.GetComponent<GameplayCursor>();
            if (gameplayCursor == null)
                gameplayCursor = playerObject.AddComponent<GameplayCursor>();
            gameplayCursor.enabled = true;
        }

        // Setup boss HP bar
        if (bossHPBar != null)
        {
            bossHPBar.maxValue = bossHP;
            bossHPBar.value = bossHP;
            bossHPBar.gameObject.SetActive(true);
        }

        // Activate boss
        if (bossObject != null)
        {
            bossObject.SetActive(true);
        }

        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    /// <summary>
    /// Call this from the boss when it takes damage.
    /// E.g. the boss has a TakeDamage(int) method that calls BossFightManager.Instance.BossTakeDamage(dmg)
    /// </summary>
    public void BossTakeDamage(int damage)
    {
        if (!fightActive) return;

        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        Debug.Log($"[BossFight] Boss took {damage} damage! HP: {currentHP}/{bossHP}");

        // Update UI
        if (bossHPBar != null)
        {
            bossHPBar.value = currentHP;
        }

        // Check if dead
        if (currentHP <= 0)
        {
            StartCoroutine(BossDefeatedRoutine());
        }
    }

    private IEnumerator BossDefeatedRoutine()
    {
        fightActive = false;
        Debug.Log("[BossFight] Boss defeated!");

        // Disable sword + cursor
        if (swordAttack != null) swordAttack.enabled = false;
        if (gameplayCursor != null) gameplayCursor.enabled = false;

        // Hide boss
        if (bossObject != null)
        {
            bossObject.SetActive(false);
        }

        // Hide HP bar
        if (bossHPBar != null)
        {
            bossHPBar.gameObject.SetActive(false);
        }

        // Brief delay before calling win
        yield return new WaitForSeconds(1.0f);

        // Tell MiniGameLauncher we won
        if (MiniGameLauncher.Instance != null)
        {
            MiniGameLauncher.Instance.WinGame();
        }
    }

    /// <summary>
    /// Optional: call to abort/reset the fight.
    /// </summary>
    public void StopFight()
    {
        fightActive = false;

        if (swordAttack != null) swordAttack.enabled = false;
        if (gameplayCursor != null) gameplayCursor.enabled = false;

        if (bossObject != null) bossObject.SetActive(false);
        if (bossHPBar != null) bossHPBar.gameObject.SetActive(false);
    }

    public bool IsFightActive()
    {
        return fightActive;
    }
}
