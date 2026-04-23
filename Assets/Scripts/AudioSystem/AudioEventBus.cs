using System;
using UnityEngine;

public static class AudioEventBus
{
    // SFX
    public static event Action<SFXEvent> OnSFXRequested;

    // Music
    public static event Action<MusicEvent> OnMusicRequested;
    public static event Action OnMusicStopped;

    // Combat
    public static event Action<CombatEvent> OnCombatStarted;
    public static event Action OnCombatEnded;

    public static void RequestSFX(SFXEvent e) => OnSFXRequested?.Invoke(e);
    public static void RequestMusic(MusicEvent e) => OnMusicRequested?.Invoke(e);
    public static void StopMusic() => OnMusicStopped?.Invoke();
    public static void StartCombat(CombatEvent e) => OnCombatStarted?.Invoke(e);
    public static void EndCombat() => OnCombatEnded?.Invoke();
}

[Serializable]
public struct SFXEvent
{
    public AudioClip Clip;
    public float Volume;
    public float Pitch;
    public Vector3? WorldPosition;

    public SFXEvent(AudioClip clip, float volume = 1f, float pitch = 1f, Vector3? pos = null)
    {
        Clip = clip;
        Volume = volume;
        Pitch = pitch;
        WorldPosition = pos;
    }
}

[Serializable]
public struct MusicEvent
{
    public AudioClip Track;
    public float FadeDuration;
    public bool Loop;

    public MusicEvent(AudioClip track, float fadeDuration = 1f, bool loop = true)
    {
        Track = track;
        FadeDuration = fadeDuration;
        Loop = loop;
    }
}

[Serializable]
public struct CombatEvent
{
    public AudioClip CombatTrack;
    public float FadeDuration;

    public CombatEvent(AudioClip track, float fadeDuration = 1f)
    {
        CombatTrack = track;
        FadeDuration = fadeDuration;
    }
}