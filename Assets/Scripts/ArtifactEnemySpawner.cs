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

    //called once from Artifact.PickUp() after artifact zone is entered & minigame is instantiated 
    public void TriggerSpawn()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        int enemiesToSpawn = Random.Range(1, 5); //1 to 4 inclusive
        List<Vector2> spawnedPositions = new List<Vector2>();

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector2? validPoint = FindValidPoint(spawnedPositions); 

            if (validPoint.HasValue) //HasValue is from the Unity API 
            {
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)]; //rando enemy
                Instantiate(prefab, validPoint.Value, Quaternion.identity);
                spawnedPositions.Add(validPoint.Value);
            }

            yield return null; //spread across frames
        }
    }

    Vector2? FindValidPoint(List<Vector2> alreadySpawned) //? is a nullable val operator, allows the var to be null
    {
        for (int attempt = 0; attempt < maxAttemptsPerEnemy; attempt++)
        {
            //generate random polar coords for a spawn candidate
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist  = Random.Range(minSpawnRadius, maxSpawnRadius);

            Vector2 candidate = (Vector2)transform.position + new Vector2(Mathf.Cos(angle)*dist, Mathf.Sin(angle)*dist);

            if (IsValid(candidate, alreadySpawned)) 
            {
                return candidate;
            }
        }

        return null; //all attempts failed, skip this enemy
    }

    bool IsValid(Vector2 point, List<Vector2> alreadySpawned)
    {
        //reject candidate if inside a wall
        if (Physics2D.OverlapCircle(point, enemyRadius, terrain))
            return false;

        //reject if wall is between artifact and spawn point
        if (Physics2D.Linecast(transform.position, point, terrain))
            return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        //min radius
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f); //red
        Gizmos.DrawWireSphere(transform.position, minSpawnRadius);

        //max radius
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.4f); //green
        Gizmos.DrawWireSphere(transform.position, maxSpawnRadius);
    }
}