using System.Collections.Generic;
using UnityEngine;

public class SFXPool
{
    private readonly Queue<AudioSource> _pool = new();
    private readonly Dictionary<string, AudioSource> trackedSFX = new(); //added too track active SFX by id
    private readonly Transform          _parent;

    public SFXPool(Transform parent, int initialSize)
    {
        _parent = parent;
        for (int i = 0; i < initialSize; i++)
            _pool.Enqueue(CreateSource());
    }

    public void Play(SFXEvent e, float masterVolume)
    {
        AudioSource src = _pool.Count > 0 ? _pool.Dequeue() : CreateSource();

        src.clip        = e.Clip;
        src.volume      = e.Volume * masterVolume;
        src.pitch       = e.Pitch;
        src.spatialBlend = e.WorldPosition.HasValue ? 1f : 0f;
        src.loop = e.Loop;

        if (e.WorldPosition.HasValue)
            src.transform.position = e.WorldPosition.Value;

        src.gameObject.SetActive(true);
        src.Play();

        // if this SFX has an ID, then track it in the dictionary to stop later.
        if (!string.IsNullOrEmpty(e.Id))
        {
            // ---> ADDED: If an ID was provided, track it manually in the dictionary.
            trackedSFX[e.Id] = src;
        }
        else
        {
            //if no id provided, just stop it when its over like normal.
            ReturnWhenDone(src, e.Clip.length / Mathf.Abs(e.Pitch));
        }
    }

    public void StopTracked(string id)
    {
        //look in dictionary for the id, if found, stop sound and return to pool
        if (trackedSFX.TryGetValue(id, out AudioSource src))
        {
            if (src != null)
            {
                src.Stop();
                src.gameObject.SetActive(false);
                _pool.Enqueue(src);
            }
            //remove from tracked dictionary 
            trackedSFX.Remove(id);
        }
    }

    private async void ReturnWhenDone(AudioSource src, float delay)
    {
        await System.Threading.Tasks.Task.Delay((int)(delay * 1000));
        if (src != null)
        {
            src.Stop();
            src.gameObject.SetActive(false);
            _pool.Enqueue(src);
        }
    }

    private AudioSource CreateSource()
    {
        var go = new GameObject("SFX_Source");
        go.transform.SetParent(_parent);
        go.SetActive(false);
        return go.AddComponent<AudioSource>();
    }
}