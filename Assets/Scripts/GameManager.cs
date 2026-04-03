using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;} //singleton

    [Header("Core")]
    [SerializeField] private UpgradeTracker upgradeTracker;
    [SerializeField] private GameStats gameStats;
    private ArtifactApplicator artifactApp;
    private SpawnPointManager spManager;

    [Header("Artifacts")]
    [SerializeField] private List<ArtifactStats> artifactDictionary;

    [Header("Economy")]
    [SerializeField] private int currentMoney = 0;
    [SerializeField] private int totalDebt = 1000000000;

    [Header("Spawning")]
    [SerializeField] private GameObject pfArtifact;
    [SerializeField] private Transform pfArtifactParent;
    [SerializeField] private int maxArtifactsInWorld = 5;
    [SerializeField] private float upgradeArtifactSpawnWeight = 0.8f;
    [SerializeField] private float SurfaceThreshold = 1f;
    [SerializeField] private float L1_Threshold = 1f;
    [SerializeField] private float L2_Threshold = 2f;
    [SerializeField] private float L3_Threshold = 3f;
    [SerializeField] private float L4_Threshold = 4f;
    [SerializeField] private float L5_Threshold = 5f; //These numbers are placeholders
    
    [Header("General")]
    [SerializeField] GameObject player;
    [SerializeField] GameObject sub;
    private Health playerHealth; 
    private Health subHealth;

    private Dictionary<GameObject, ArtifactSpawnPoint> artifactsActive = new Dictionary<GameObject, ArtifactSpawnPoint>();

    //Expose 
    public float L5_THRESH => L5_Threshold;
    public float L4_THRESH => L4_Threshold;
    public float L3_THRESH => L3_Threshold;
    public float L2_THRESH => L2_Threshold;
    public float L1_THRESH => L1_Threshold;



    //EVENTS
    public UnityEvent<int, int> onMoneyChanged;
    public UnityEvent<ArtifactStats> onArtifactCollected;

    //PLANNING 
    // 1 - Game Over Logic
    // 2 - Keeps track of money accumualated and debt owed
    // 3 - Spawns enemies 
    // 4 - Spawns Artifacts based on current player stats and what they need
    // 5 - Changes player stats when upgrades are attained
    // 6 - Feeds dialogue when certain checkpoints are attained. 
    // 7 - Handles hiding and relocation of submarine when it gets "destroyed"

    //STARTUP/SHUTDOWN STUFF
    ////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        playerHealth = player.GetComponent<Health>();
        subHealth = sub.GetComponent<Health>();
        artifactApp = GetComponent<ArtifactApplicator>();
        spManager = GetComponent<SpawnPointManager>();
        AddMoney(currentMoney);
    }

    private void Start()
    {
        SpawnInitialArtifacts();
        upgradeTracker.ResetAll();
        gameStats.ResetStats();

    }

    private void OnEnable()
    {
        playerHealth.died.AddListener(EndGame);
        subHealth.died.AddListener(EndGame);
    }

    private void OnDisable()
    {
        playerHealth.died.RemoveListener(EndGame);
        subHealth.died.RemoveListener(EndGame);
    }

    //COLLECTION
    ////////////////////////////////////////////////////////////////////////////////////

    public void CollectArtifact(Artifact artifact)
    {
        ArtifactStats stats = artifact.Stats; //grab stats of collected artifact
        artifactApp.ApplyArtifact(stats); //process artifact

        AddMoney(stats.sellValue);

        FreeUpSpawnPoint(artifact);
        SpawnArtifactBasedonNeeds();
    }

    //ARTIFACT SPAWNING 
    ////////////////////////////////////////////////////////////////////////////////////
    private void SpawnInitialArtifacts()
    {
        int artifactsToSpawn = Mathf.Min(maxArtifactsInWorld, spManager.GetTotalSpawnPoints());

        for (int i = 0; i < artifactsToSpawn; i++)
        {
            SpawnArtifactBasedonNeeds();
        }
    }

    private void SpawnArtifactBasedonNeeds() //The based on needs function doesnt really exist yet 
    {
        ArtifactStats selectedArtifact = null;
        ArtifactSpawnPoint spawnPoint = null;
        int maxAttempts = 20;
        int attempts = 0;

        if (artifactsActive.Count >= maxArtifactsInWorld)
            return;

        while (spawnPoint == null && attempts < maxAttempts)
        {
            attempts++;
            float roll = Random.value;

            if (roll < upgradeArtifactSpawnWeight) //spawn upgrade artifact 
            {
                List<ArtifactStats> upgradeArtifacts = GetUpgradeArtifacts();

                if(upgradeArtifacts.Count > 0)
                    selectedArtifact = upgradeArtifacts[Random.Range(0, upgradeArtifacts.Count)];

                else //in scenario that all upgrades are maxed 
                {
                    List<ArtifactStats> replenishArtifacts = GetReplenishArtifacts();
                    selectedArtifact = replenishArtifacts[Random.Range(0, replenishArtifacts.Count)];
                }
            }
        
            else //spawn replenish artifact
            {
                List<ArtifactStats> replenishArtifacts = GetReplenishArtifacts();

                if (replenishArtifacts.Count > 0)
                    selectedArtifact = replenishArtifacts[Random.Range(0, replenishArtifacts.Count)];

            }

            spawnPoint = spManager.GetSpawnPointForArtifact(selectedArtifact);

            if (spawnPoint == null) //reset selected Artifact to try again
            {
                selectedArtifact = null;
            }
            
            if(spawnPoint != null && selectedArtifact != null)
            {
                SpawnArtifact(selectedArtifact, spawnPoint);
            }

            //List<ArtifactStats> spawnableArtifacts = GetAllSpawnableArtifacts();
            //selectedArtifact = spawnableArtifacts[Random.Range(0, spawnableArtifacts.Count)];
        }
    }

    private void SpawnArtifact(ArtifactStats selectedArtifact, ArtifactSpawnPoint spawnPoint)
    {
        Vector2 spawnPos = spawnPoint.SpawnPosition;
        GameObject artifactObj = Instantiate(pfArtifact, spawnPos, Quaternion.identity, pfArtifactParent);

        Artifact artifact = artifactObj.GetComponent<Artifact>();
        artifact.SetStats(selectedArtifact);

        spawnPoint.SetOccupied(true);
        artifactsActive.Add(artifactObj, spawnPoint);
    }

    private void FreeUpSpawnPoint(Artifact artifact)
    {
        if(artifactsActive.TryGetValue(artifact.gameObject, out ArtifactSpawnPoint spawnPoint))
        {
            spawnPoint.SetOccupied(false);
            artifactsActive.Remove(artifact.gameObject);
        }
    }

    //ENEMY SPAWNING
    ////////////////////////////////////////////////////////////////////////////////////

    //ECONOMY
    ////////////////////////////////////////////////////////////////////////////////////
    private void AddMoney(int amount)
    {
        currentMoney += amount;
        totalDebt -= amount;
        onMoneyChanged?.Invoke(currentMoney, totalDebt);
    }


    //GAME STATE STUFF
    ////////////////////////////////////////////////////////////////////////////////////
    
    private void WinGame()
    {
        //aint no way you're winning lmao
        Debug.Log("You Win!");
        RestartGame();
    }

    private void EndGame()
    {
        //plcaeholder crap
        Time.timeScale = 0f;
        Debug.Log("Game Over!");
        RestartGame();
    } 

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    //HELPERS
    ////////////////////////////////////////////////////////////////////////////////////
    
    private List<ArtifactStats> GetAllSpawnableArtifacts()
    {
        List<ArtifactStats> allSpawnable = new List<ArtifactStats>();
        allSpawnable.AddRange(GetUpgradeArtifacts());
        allSpawnable.AddRange(GetReplenishArtifacts());
        return allSpawnable;
    }
    
    private List<ArtifactStats> GetUpgradeArtifacts()
    {
        return artifactDictionary.Where(a => upgradeTracker.IsUpgradableArtifact(a.artifactType)).ToList();
    }

    private List<ArtifactStats> GetReplenishArtifacts()
    {
        return artifactDictionary.Where(a => !upgradeTracker.IsUpgradableArtifact(a.artifactType)).ToList();
    }

    private void OnDrawGizmos()
    {
        DrawThresholdGizmo(SurfaceThreshold, Color.azure, "Surface");
        DrawThresholdGizmo(L1_Threshold, Color.green, "L1");
        DrawThresholdGizmo(L2_Threshold, Color.cyan, "L2");
        DrawThresholdGizmo(L3_Threshold, Color.yellow, "L3");
        DrawThresholdGizmo(L4_Threshold, new Color(1f, 0.5f, 0f), "L4"); // orange
        DrawThresholdGizmo(L5_Threshold, Color.red, "L5");
    }

    private void DrawThresholdGizmo(float height, Color color, string label)
    {
        Gizmos.color = color;

        float lineLength = 1000f; // how wide the line is in world units
        Vector3 start = new Vector3(transform.position.x - lineLength * 0.5f, height, transform.position.z);
        Vector3 end   = new Vector3(transform.position.x + lineLength * 0.5f, height, transform.position.z);

        Gizmos.DrawLine(start, end);
    }


}
