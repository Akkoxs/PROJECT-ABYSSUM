using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Submarine : MonoBehaviour
{
    [SerializeField] private GameObject enterExitPoint; // Where player spawns when exiting
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private GameObject player;

    private PlayerController playerController;
    private SpriteRenderer playerSprite;
    private EnterExitSubmarine ees;
    private bool playerInside = false;
    private float moveSpeed = 5f;

    //public read only vars
    public bool PlayerInside => playerInside;

    public UnityEvent enteredSubmarine;
    public UnityEvent exitedSubmarine;
    
    private void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerSprite = player.GetComponent<SpriteRenderer>();
        ees = enterExitPoint.GetComponent<EnterExitSubmarine>();
    }

    private void Update()
    {
        if (ees.playerInRange && !playerInside && Keyboard.current[interactKey].wasPressedThisFrame)
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
        player.transform.position = enterExitPoint.transform.position;
        playerController.enabled = true;
        playerSprite.enabled = true;
        Debug.Log("Exited submarine!");
    }


}