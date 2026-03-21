using UnityEngine;

public class MinigameProgressManager : MonoBehaviour
{
    public bool minigameCompleted = false;
    public bool minigameActive = false;

    private static MinigameProgressManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public static MinigameProgressManager Instance
    {
        get { return instance; }
    }

    public void SetMinigameStarted()
    {
        minigameActive = true;
        Debug.Log("Minigame started in main scene");
        //OnMinigameStarted();
    }

    public void SetMinigameCompleted()
    {
        minigameCompleted = true;
        minigameActive = false;
        Debug.Log("Minigame marked as completed in main scene");
        //OnMinigameCompleted();
    }

    //void OnMinigameStarted()
    //{

    //}

    //void OnMinigameCompleted()
    //{
    //}
}