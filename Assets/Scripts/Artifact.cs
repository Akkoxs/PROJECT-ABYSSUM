using UnityEngine;
using TMPro;

public class Artifact : MonoBehaviour, IRadarDetectable
{
    [SerializeField] private ArtifactStats artifactStats;
    [SerializeField] private float spriteScale;
    [SerializeField] private ArtifactPopup pfArtifactPopup;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;
    private BoxCollider2D collider;
    private bool isCollected = false; 

    public ArtifactStats Stats => artifactStats;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
            PickUp();
    }

    public void SetStats(ArtifactStats stats)
    {
        
        artifactStats = stats;

        spriteRenderer = GetComponent<SpriteRenderer>();  //render when stats are set 
        collider = GetComponent<BoxCollider2D>();
        spriteRenderer.sprite = artifactStats.icon;
        transform.localScale = Vector3.one * spriteScale;
    }

    public void PickUp()
    {
        isCollected = true;

        //spawn popup
        ArtifactPopup popup = Instantiate(pfArtifactPopup, transform.position, Quaternion.identity);
        popup.Setup(artifactStats.artifactName, artifactStats.sellValue);

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
