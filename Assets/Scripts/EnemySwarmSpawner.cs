using System.Collections.Generic;
using UnityEngine;

public class EnemySwarmSpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 2f; 
    [SerializeField] private float initialDelay = 0.5f; 

    private float spawnTimer = 0f;
    private bool isSpawning = false;
    private bool minigameWasActive = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("No enemy prefab assigned");
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned");
        }

        spawnTimer = initialDelay;
    }

    void Update()
    {
        if (MinigameProgressManager.Instance == null) return;

        bool minigameActive = MinigameProgressManager.Instance.minigameActive;
        bool minigameCompleted = MinigameProgressManager.Instance.minigameCompleted;

        if (minigameActive && !minigameWasActive)
        {
            OnMinigameStarted();
        }

        if (minigameCompleted && isSpawning)
        {
            OnMinigameCompleted();
        }

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
        Debug.Log("Swarm started");
        isSpawning = true;
        spawnTimer = initialDelay;
    }

    void OnMinigameCompleted()
    {
        Debug.Log("Swarm completed");
        isSpawning = false;
        // DestroyAllSpawnedEnemies();
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints.Count == 0) return;

        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[randomIndex];

        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn point at index " + randomIndex + " is null!");
            return;
        }

        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedEnemies.Add(spawnedEnemy);

        Debug.Log("Spawned enemy at index " + randomIndex + " with name " + spawnPoint.name);
    }

    //void DestroyAllSpawnedEnemies()
    //{
    //    foreach (GameObject enemy in spawnedEnemies)
    //    {
    //        if (enemy != null)
    //        {
    //            Destroy(enemy);
    //        }
    //    }

    //    spawnedEnemies.Clear();
    //    Debug.Log("Destroyed all spawned enemies");
    //}

    //public void StartSpawning()
    //{
    //    isSpawning = true;
    //    spawnTimer = initialDelay;
    //}

    //public void StopSpawning()
    //{
    //    isSpawning = false;
    //}

    void OnDrawGizmos()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return;

        Gizmos.color = Color.red;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 1f);
            }
        }
    }
}