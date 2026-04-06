using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MinigameScene : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; 
    [SerializeField] private bool oneTimeUse = true;
    
    private string playerTag = "Player";
    private PlayerInput playerInput;
    private bool trigger = false;
    private Scene loadedScene;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag) && (!trigger || !oneTimeUse))
        {
            LoadAdditiveScene();
            //playerInput = collision.GetComponent<PlayerInput>();
            //playerInput.enabled = false;

            if (oneTimeUse)
            {
                trigger = true;
            }
        }
    }

    private void LoadAdditiveScene()
    {
        Scene checkScene = SceneManager.GetSceneByName(sceneToLoad);
        if (checkScene.isLoaded)
        {
            Debug.LogWarning(sceneToLoad + " is already loaded");
            return;
        }

        SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive).completed += OnSceneLoaded;
        Debug.Log("Loading minigame scene: " + sceneToLoad);
    }

    private void OnSceneLoaded(AsyncOperation asyncOperation)
    {
        loadedScene = SceneManager.GetSceneByName(sceneToLoad);
        Debug.Log("Minigame scene loaded: " + sceneToLoad);
    }

    public void UnloadAdditiveScene()
    {
        if (loadedScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(loadedScene).completed += OnSceneUnloaded;
        }
    }

    private void OnSceneUnloaded(AsyncOperation asyncOperation)
    {
        Debug.Log("Minigame scene unloaded");
    }
}