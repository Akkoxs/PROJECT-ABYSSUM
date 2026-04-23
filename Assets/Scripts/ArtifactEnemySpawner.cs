using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Area Settings")]
    public float minSpawnRadius = 2f;
    public float maxSpawnRadius = 6f;
    public LayerMask terrain;
    public float enemyRadius = 0.4f;

    [Header("Attempts")]
    public int maxAttemptsPerEnemy = 30;

    public void TriggerSpawn()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        int enemiesToSpawn = Random.Range(1, 5);
        List<Vector2> spawnedPositions = new List<Vector2>();

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector2? validPoint = FindValidPoint(spawnedPositions);

            if (validPoint.HasValue)
            {
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                Instantiate(prefab, validPoint.Value, Quaternion.identity);
                spawnedPositions.Add(validPoint.Value);
            }

            yield return null;
        }
    }

    Vector2? FindValidPoint(List<Vector2> alreadySpawned)
    {
        for (int attempt = 0; attempt < maxAttemptsPerEnemy; attempt++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(minSpawnRadius, maxSpawnRadius);

            Vector2 candidate = (Vector2)transform.position + new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);

            if (IsValid(candidate, alreadySpawned))
            {
                return candidate;
            }
        }

        return null;
    }

    bool IsValid(Vector2 point, List<Vector2> alreadySpawned)
    {
        if (Physics2D.OverlapCircle(point, enemyRadius, terrain))
            return false;

        if (Physics2D.Linecast(transform.position, point, terrain))
            return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, minSpawnRadius);

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, maxSpawnRadius);
    }
}