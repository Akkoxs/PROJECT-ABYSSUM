// AudioLibrary.cs
// ScriptableObject — assign clips in the Inspector, reference by name.
// Attach a reference to AudioManager if you want name-based lookups.
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Library")]
public class AudioLibrary : ScriptableObject
{
    [Serializable]
    public struct Entry { public string Key; public AudioClip Clip; }

    [SerializeField] private Entry[] entries;

    private Dictionary<string, AudioClip> _map;

    public void Init()
    {
        _map = new Dictionary<string, AudioClip>();
        foreach (var e in entries) _map[e.Key] = e.Clip;
    }

    public AudioClip Get(string key) =>
        _map.TryGetValue(key, out var clip) ? clip : null;
}