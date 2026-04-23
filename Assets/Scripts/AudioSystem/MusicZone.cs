using UnityEngine;

public class MusicZone : MonoBehaviour
{
    [SerializeField] private AudioClip zoneMusic;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private Submarine submarine;

    public static AudioClip LastZoneTrack { get; private set; }
    public static float LastZoneFade { get; private set; }
    public static bool InCombat { get; set; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (submarine != null)
        {
            if (other.CompareTag("Player") && submarine.PlayerInside) return;
            if (other.CompareTag("Submarine") && !submarine.PlayerInside) return;
        }

        if (other.CompareTag("Player") || other.CompareTag("Submarine"))
        {
            LastZoneTrack = zoneMusic;
            LastZoneFade = fadeDuration;

            if (InCombat) return;

            AudioEventBus.RequestMusic(new MusicEvent(zoneMusic, fadeDuration));
        }
    }
}