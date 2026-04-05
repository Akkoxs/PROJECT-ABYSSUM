using UnityEngine;

public class MusicZone : MonoBehaviour
{
    [SerializeField] private AudioClip zoneMusic;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private Submarine submarine;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (submarine != null)
        {
            if (other.CompareTag("Player") && submarine.PlayerInside) return;
            if (other.CompareTag("Submarine") && !submarine.PlayerInside) return;
        }

        if (other.CompareTag("Player") || other.CompareTag("Submarine"))
        {
            AudioEventBus.RequestMusic(new MusicEvent(zoneMusic, fadeDuration));
        }
    }
}