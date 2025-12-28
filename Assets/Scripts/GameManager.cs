using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject player;

    private Health playerHealth; 

    //PLANNING 
    // 1 - Game Over Logic
    // 2 - Keeps track of money accumualated and debt owed
    // 3 - Spawns enemies 
    // 4 - Spawns Artifacts based on current player stats and what they need
    // 5 - Changes player stats when upgrades are attained
    // 6 - Feeds dialogue when certain checkpoints are attained. 
    // 7 - Handles hiding and relocation of submarine when it gets "destroyed"


    private void Awake()
    {
        playerHealth = player.GetComponent<Health>();
    }

    private void OnEnable()
    {
        playerHealth.died.AddListener(EndGame);
    }

    private void OnDisable()
    {
        playerHealth.died.RemoveListener(EndGame);
    }

    private void EndGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Game Over!");
        //Load gameOver screen?
        //Actual game over logic can go here 
    } 
}
