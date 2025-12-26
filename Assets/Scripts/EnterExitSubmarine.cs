using UnityEngine;

public class EnterExitSubmarine : MonoBehaviour
{

    private bool player_in_range;
    public bool playerInRange => player_in_range;  

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            player_in_range = true;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            player_in_range = false;
    }
}
