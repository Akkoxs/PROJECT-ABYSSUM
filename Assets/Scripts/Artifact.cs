using UnityEngine;

public class Artifact : MonoBehaviour, IRadarDetectable
{
    [SerializeField] private ArtifactStats artifactStats;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    public ArtifactStats Stats => artifactStats;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PickUp();
    }

    public void SetStats(ArtifactStats stats)
    {
        
        artifactStats = stats;

        spriteRenderer = GetComponent<SpriteRenderer>();  //render when stats are set 
        spriteRenderer.sprite = artifactStats.icon;

    }

    public void PickUp()
    {
        gameManager.CollectArtifact(this);
        Destroy(gameObject);
    }

    public RadarObjectType GetObjectType()
    {
        return RadarObjectType.Artifact;
    }

    public string GetRadarDisplayName()
    {
        return artifactStats.artifactName;
    }



}
