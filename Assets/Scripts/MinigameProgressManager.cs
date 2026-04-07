using System.Collections.Generic;
using UnityEngine;

public class MinigameProgressManager : MonoBehaviour
{
    private static MinigameProgressManager instance;
    public static MinigameProgressManager Instance => instance;

    // Tracks state per minigame ID
    private Dictionary<string, bool> activeStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> completedStates = new Dictionary<string, bool>();

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    public void SetMinigameStarted(string id)
    {
        activeStates[id] = true;
        completedStates[id] = false;
        Debug.Log($"Minigame '{id}' started");
    }

    public void SetMinigameCompleted(string id)
    {
        activeStates[id] = false;
        completedStates[id] = true;
        Debug.Log($"Minigame '{id}' completed");
    }

    public bool IsActive(string id)
    {
        return activeStates.TryGetValue(id, out bool v) && v;
    }

    public bool IsCompleted(string id)
    {
        return completedStates.TryGetValue(id, out bool v) && v;
    }
}