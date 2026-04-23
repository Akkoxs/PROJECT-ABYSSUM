using System.Collections;
using UnityEngine;

public class CombatMusicManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float checkInterval = 1f;
    [SerializeField] private float enemySearchRadius = 30f;
    [SerializeField] private LayerMask enemyLayer;

    private bool inCombat = false;
    private Coroutine _checkRoutine;
    private Transform _playerTransform;

    void Awake()
    {
        AudioEventBus.OnCombatStarted += HandleCombatStart;
        AudioEventBus.OnCombatEnded += HandleCombatEnd;
    }

    void OnDestroy()
    {
        AudioEventBus.OnCombatStarted -= HandleCombatStart;
        AudioEventBus.OnCombatEnded -= HandleCombatEnd;
    }

    private void HandleCombatStart(CombatEvent e)
    {
        Debug.Log("Combat started! Track: " + e.CombatTrack);
        if (inCombat) return;
        inCombat = true;
        MusicZone.InCombat = true;

        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        AudioEventBus.RequestMusic(new MusicEvent(e.CombatTrack, e.FadeDuration));

        if (_checkRoutine != null) StopCoroutine(_checkRoutine);
        _checkRoutine = StartCoroutine(CheckForEnemies());
    }

    private void HandleCombatEnd()
    {
        if (!inCombat) return;
        inCombat = false;
        MusicZone.InCombat = false;

        if (_checkRoutine != null)
        {
            StopCoroutine(_checkRoutine);
            _checkRoutine = null;
        }

        if (MusicZone.LastZoneTrack != null)
            AudioEventBus.RequestMusic(new MusicEvent(MusicZone.LastZoneTrack, MusicZone.LastZoneFade));
        else
            AudioEventBus.StopMusic();
    }

    private IEnumerator CheckForEnemies()
    {
        while (inCombat)
        {
            yield return new WaitForSeconds(checkInterval);

            if (_playerTransform == null) continue;

            Collider2D[] nearby = Physics2D.OverlapCircleAll(
                _playerTransform.position,
                enemySearchRadius,
                enemyLayer
            );

            if (nearby.Length == 0)
            {
                AudioEventBus.EndCombat();
            }
        }
    }
}