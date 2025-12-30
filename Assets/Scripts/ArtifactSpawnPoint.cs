using Unity.VisualScripting;
using UnityEngine;

public class ArtifactSpawnPoint : MonoBehaviour
{   
    private GameManager gameManager;

    private int spawnLevel;
    private bool isOccupied;

    [SerializeField] private float spawnRadius = 2f;

    //Expose
    [SerializeField] public int SpawnLevel => spawnLevel;
    public bool IsOccupied => isOccupied;
    public Vector2 SpawnPosition => (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        AssignSpawnLevel();    
    }

    private void AssignSpawnLevel()
    {
        float currPos = transform.position.y;

         if (currPos > gameManager.L1_THRESH)
            spawnLevel = 1;
        else if (currPos > gameManager.L2_THRESH)
            spawnLevel = 2;
        else if (currPos > gameManager.L3_THRESH)
            spawnLevel = 3;
        else if (currPos > gameManager.L4_THRESH)
            spawnLevel = 4;
        else if (currPos > gameManager.L5_THRESH)
            spawnLevel = 5;
        else
        {
            Debug.Log("Artifact out of bounds");
            spawnLevel = 5;
        }
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }

    private void OnDrawGizmos()
    {
        // Color based on level (once assigned)
        Color color = spawnLevel switch
        {
            1 => Color.green,    // Shallow/safe
            2 => Color.yellow,
            3 => new Color(1f, 0.5f, 0f), // Orange
            4 => Color.red,
            5 => Color.magenta,  // Deep/dangerous
            _ => Color.white
        };
        
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

         if (isOccupied)
            Gizmos.DrawSphere(transform.position, spawnRadius);
    }

}
