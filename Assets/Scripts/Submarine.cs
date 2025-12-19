using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Submarine : MonoBehaviour
{
    [SerializeField] private Transform exitPoint; // Where player spawns when exiting
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private GameObject player;

    private SimplePlayerController playerController;
    private SpriteRenderer playerSprite;
    private bool playerInRange = false;
    private bool playerInside = false;
    private float moveSpeed = 5f;

    //public read only vars
    public bool PlayerInside => playerInside;

    public UnityEvent enteredSubmarine;
    public UnityEvent exitedSubmarine;
    
    private void Start()
    {
        playerController = player.GetComponent<SimplePlayerController>();
        playerSprite = player.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (playerInRange && !playerInside && Keyboard.current[interactKey].wasPressedThisFrame)
            EnterSubmarine();

        else if (playerInside && Keyboard.current[interactKey].wasPressedThisFrame)
            ExitSubmarine();
        
        if (playerInside)
        {
            float vertical = 0f; 

            if (Keyboard.current.wKey.isPressed)
                vertical = 1f;
            
            else if (Keyboard.current.sKey.isPressed)
                vertical = -1f;

            transform.Translate(Vector2.up * vertical * moveSpeed * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
    
    public void EnterSubmarine()
    {        
        playerInside = true;
        enteredSubmarine?.Invoke();
        playerController.enabled = false;
        playerSprite.enabled = false;
        Debug.Log("Entered submarine!");
    }
    
    public void ExitSubmarine()
    {
        if (!playerInside) return;
        
        playerInside = false;
        exitedSubmarine?.Invoke();
        player.transform.position = exitPoint.position;
        playerController.enabled = true;
        playerSprite.enabled = true;
        Debug.Log("Exited submarine!");
    }


}