using System;
using UnityEngine;

public static class AudioEventBus
{
    // SFX
    public static event Action<SFXEvent> OnSFXRequested;
    
    // Music
    public static event Action<MusicEvent> OnMusicRequested;
    public static event Action OnMusicStopped;

    public static void RequestSFX(SFXEvent e)   => OnSFXRequested?.Invoke(e);
    public static void RequestMusic(MusicEvent e) => OnMusicRequested?.Invoke(e);
    public static void StopMusic()               => OnMusicStopped?.Invoke();
}

// Lightweight data structs — no MonoBehaviour coupling
[Serializable]
public struct SFXEvent
{
    public AudioClip Clip;
    public float     Volume;       // 0–1, default 1
    public float     Pitch;        // default 1
    public Vector3?  WorldPosition; // null = 2D/UI sound

    public SFXEvent(AudioClip clip, float volume = 1f, float pitch = 1f, Vector3? pos = null)
    {
        Clip          = clip;
        Volume        = volume;
        Pitch         = pitch;
        WorldPosition = pos;
    }
}

[Serializable]
public struct MusicEvent
{
    public AudioClip Track;
    public float     FadeDuration; // seconds
    public bool      Loop;

    public MusicEvent(AudioClip track, float fadeDuration = 1f, bool loop = true)
    {
        Track        = track;
        FadeDuration = fadeDuration;
        Loop         = loop;
    }
}