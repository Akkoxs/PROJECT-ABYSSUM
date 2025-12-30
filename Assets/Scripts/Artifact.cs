using UnityEngine;

public class Artifact : MonoBehaviour, IRadarDetectable
{
    [SerializeField] private ArtifactStats artifactStats;
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
        FitSpriteToBounds(spriteRenderer);
    }

    public void PickUp()
    {
        isCollected = true;
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

    private Vector2 GetColliderWorldSize()
    {
        if (collider == null)
            return Vector2.one;

        Vector2 localSize = collider.size;
        Vector3 scale = transform.lossyScale;

        return new Vector2(localSize.x * scale.x, localSize.y * scale.y);
    }


    private void FitSpriteToBounds(SpriteRenderer sr)
    {
        if (sr.sprite == null || collider == null) 
            return;

        Vector2 colliderSize = GetColliderWorldSize();
        Vector2 spriteSize = sr.sprite.bounds.size;

        float scaleX = colliderSize.x / spriteSize.x;
        float scaleY = colliderSize.y / spriteSize.y;

        float scale = Mathf.Min(scaleX, scaleY);

        sr.transform.localScale = Vector3.one * scale;
    }


}
