using UnityEngine;

public class GameOver : MonoBehaviour
{
    //handles Game Over 
    [SerializeField] Health player;

    private void OnEnable()
    {
        player.died.AddListener(EndGame);
    }

    private void OnDisable()
    {
        player.died.RemoveListener(EndGame);
    }


    private void EndGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Game Over!");
        //Actual game over logic can go here 
    } 
}
