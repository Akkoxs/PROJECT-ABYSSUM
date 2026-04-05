using System.Collections;
using UnityEngine;

public class AudioMasterManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int   sfxPoolSize    = 10;
    [SerializeField] private float masterSFXVol   = 1f;
    [SerializeField] private float masterMusicVol = 0.6f;

    // --- Internal ---
    private SFXPool   _sfxPool;
    private AudioSource _musicSource;
    private Coroutine   _fadeRoutine;

    // ------------------------------------------------------------------ //
    void Awake()
    {
        _sfxPool     = new SFXPool(transform, sfxPoolSize);
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.volume = masterMusicVol;

        // Subscribe to the bus
        AudioEventBus.OnSFXRequested   += HandleSFX;
        AudioEventBus.OnMusicRequested += HandleMusic;
        AudioEventBus.OnMusicStopped   += HandleStopMusic;
    }

    void OnDestroy()
    {
        AudioEventBus.OnSFXRequested   -= HandleSFX;
        AudioEventBus.OnMusicRequested -= HandleMusic;
        AudioEventBus.OnMusicStopped   -= HandleStopMusic;
    }

    // ------------------------------------------------------------------ //
    private void HandleSFX(SFXEvent e)
    {
        if (e.Clip == null) return;
        _sfxPool.Play(e, masterSFXVol);
    }

    private void HandleMusic(MusicEvent e)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(CrossfadeMusic(e));
    }

    private void HandleStopMusic()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOut(_musicSource, 1f));
    }

    // ------------------------------------------------------------------ //
    private IEnumerator CrossfadeMusic(MusicEvent e)
    {
        // Fade out current
        yield return FadeOut(_musicSource, e.FadeDuration * 0.5f);

        _musicSource.clip = e.Track;
        _musicSource.loop = e.Loop;
        _musicSource.Play();

        // Fade in new
        yield return FadeIn(_musicSource, masterMusicVol, e.FadeDuration * 0.5f);
    }

    private IEnumerator FadeOut(AudioSource src, float duration)
    {
        float start = src.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            src.volume = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        src.volume = 0f;
        src.Stop();
    }

    private IEnumerator FadeIn(AudioSource src, float target, float duration)
    {
        src.volume = 0f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            src.volume = Mathf.Lerp(0f, target, t / duration);
            yield return null;
        }
        src.volume = target;
    }
}