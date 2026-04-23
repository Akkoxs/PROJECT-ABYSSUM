using System.Collections;
using UnityEngine;

public class CombatDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float checkInterval = 1f;
    [SerializeField] private float enemySearchRadius = 15f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private AudioClip combatMusic;
    [SerializeField] private float fadeDuration = 1f;

    [Header("References")]
    [SerializeField] private Submarine submarine;

    private bool inCombat = false;

    void Start()
    {
        StartCoroutine(ScanForEnemies());
    }

    private IEnumerator ScanForEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            Vector2 scanPosition = (submarine != null && submarine.PlayerInside)
                ? (Vector2)submarine.transform.position
                : (Vector2)transform.position;

            Collider2D[] nearby = Physics2D.OverlapCircleAll(
                scanPosition,
                enemySearchRadius,
                enemyLayer
            );

            Debug.Log("Scanning at: " + scanPosition + " found: " + nearby.Length + " enemies");

            bool enemiesNearby = nearby.Length > 0;

            if (enemiesNearby && !inCombat)
            {
                inCombat = true;
                MusicZone.InCombat = true;
                AudioEventBus.RequestMusic(new MusicEvent(combatMusic, fadeDuration));
                Debug.Log("Combat started!");
            }
            else if (!enemiesNearby && inCombat)
            {
                inCombat = false;
                MusicZone.InCombat = false;
                if (MusicZone.LastZoneTrack != null)
                    AudioEventBus.RequestMusic(new MusicEvent(MusicZone.LastZoneTrack, MusicZone.LastZoneFade));
                else
                    AudioEventBus.StopMusic();
                Debug.Log("Combat ended!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemySearchRadius);
    }
}