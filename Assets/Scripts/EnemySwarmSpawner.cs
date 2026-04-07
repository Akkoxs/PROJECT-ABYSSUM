using System.Collections.Generic;
using UnityEngine;

public class EnemySwarmSpawner : MonoBehaviour
{
    [Header("Minigame Link")]
    [SerializeField] private string minigameID; // must match the ID set on MinigameTrigger

    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 7f;
    [SerializeField] private float initialDelay = 0.5f;

    private float spawnTimer = 0f;
    private bool isSpawning = false;
    private bool minigameWasActive = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        if (enemyPrefab == null) Debug.LogError($"[{name}] No enemy prefab assigned");
        if (spawnPoints.Count == 0) Debug.LogError($"[{name}] No spawn points assigned");
        if (string.IsNullOrEmpty(minigameID)) Debug.LogError($"[{name}] No minigame ID assigned");
        spawnTimer = initialDelay;
    }

    void Update()
    {
        if (MinigameProgressManager.Instance == null || string.IsNullOrEmpty(minigameID)) return;

        bool minigameActive = MinigameProgressManager.Instance.IsActive(minigameID);
        bool minigameCompleted = MinigameProgressManager.Instance.IsCompleted(minigameID);

        if (minigameActive && !minigameWasActive)
            OnMinigameStarted();

        if (minigameCompleted && isSpawning)
            OnMinigameCompleted();

        minigameWasActive = minigameActive;

        if (isSpawning)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = spawnInterval;
            }
        }
    }

    void OnMinigameStarted()
    {
        Debug.Log($"[{minigameID}] Swarm started");
        isSpawning = true;
        spawnTimer = initialDelay;
    }

    void OnMinigameCompleted()
    {
        Debug.Log($"[{minigameID}] Swarm stopped");
        isSpawning = false;
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints.Count == 0) return;

        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[randomIndex];

        if (spawnPoint == null) { Debug.LogWarning("Null spawn point at index " + randomIndex); return; }

        spawnedEnemies.Add(Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation));
    }

    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.red;
        foreach (Transform sp in spawnPoints)
        {
            if (sp == null) continue;
            Gizmos.DrawWireSphere(sp.position, 0.5f);
            Gizmos.DrawLine(sp.position, sp.position + Vector3.up * 1f);
        }
    }
}