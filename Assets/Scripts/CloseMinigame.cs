using UnityEngine;
using UnityEngine.SceneManagement;

public class CloseMinigame : MonoBehaviour
{
    public void CloseScene()
    {
        Time.timeScale = 1f;
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }
}
