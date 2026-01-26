using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject obstaclePrefab;
    public GameObject collectiblePrefab;

    [Header("Spawn Settings")]
    public float minSpawnInterval = 1.5f;
    public float maxSpawnInterval = 3f;
    public float collectibleChance = 0.3f; // 30% chance to spawn collectible instead of obstacle

    [Header("Positioning")]
    public Transform spawnPoint;
    public float groundY = 0f;
    public float airY = 100f; // For flying obstacles/collectibles

    private float spawnTimer;
    private float nextSpawnTime;

    void Start()
    {
        ResetSpawnTimer();
    }

    void Update()
    {
        if (DinoGameManager.Instance != null && DinoGameManager.Instance.IsGameOver())
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= nextSpawnTime)
        {
            Spawn();
            ResetSpawnTimer();
        }
    }

    void Spawn()
    {
        // Decide: collectible or obstacle
        bool spawnCollectible = Random.value < collectibleChance;
        GameObject prefab = spawnCollectible ? collectiblePrefab : obstaclePrefab;

        if (prefab == null)
        {
            Debug.LogWarning("[ObstacleSpawner] Prefab is null!");
            return;
        }

        // Random height (ground or air)
        float yPos = Random.value > 0.5f ? groundY : airY;

        // Use the Spawner's position as base, but we need to respect the Parent's coordinate space
        // Best approach for UI: Instantiate as child, then set anchoredPosition
        GameObject spawned = Instantiate(prefab, transform);
        
        RectTransform rt = spawned.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Reset position to (0,0,0) relative to parent first
            rt.anchoredPosition = Vector2.zero;
            
            // Set X to 0 (start at spawner's X) and Y to the target height
            // Assuming ObstacleSpawner is placed at the right edge of the screen
            rt.anchoredPosition = new Vector2(0, yPos);
        }
        else
        {
            // Fallback for non-UI
            spawned.transform.localPosition = new Vector3(0, yPos, 0);
        }
        
        // Set tag
        if (spawnCollectible)
        {
            spawned.tag = "Collectible";
            Debug.Log($"[ObstacleSpawner] Spawned collectible at Y={yPos}");
        }
        else
        {
            spawned.tag = "Obstacle";
            Debug.Log($"[ObstacleSpawner] Spawned obstacle at Y={yPos}");
        }

        // Add movement script
        DinoObstacle obstacleScript = spawned.GetComponent<DinoObstacle>();
        if (obstacleScript == null)
        {
            obstacleScript = spawned.AddComponent<DinoObstacle>();
        }
    }

    void ResetSpawnTimer()
    {
        spawnTimer = 0;
        nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}
