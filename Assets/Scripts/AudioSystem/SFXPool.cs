using System.Collections.Generic;
using UnityEngine;

public class SFXPool
{
    private readonly Queue<AudioSource> _pool = new();
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

        if (e.WorldPosition.HasValue)
            src.transform.position = e.WorldPosition.Value;

        src.gameObject.SetActive(true);
        src.Play();

        // Return to pool after clip finishes
        // (Uses a simple polling approach via AudioManager's Update,
        //  or you can do a coroutine — shown below as a static helper)
        ReturnWhenDone(src, e.Clip.length / Mathf.Abs(e.Pitch));
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