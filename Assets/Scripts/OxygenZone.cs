using UnityEngine;

//this is terrible but it should work

public class OxygenZone : MonoBehaviour
{

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject sub;
    [SerializeField] private Collider2D subAirProbe; //for some reason this needs to be here

    private Collider2D playerAirProbe;
    private PlayerOxygen playerOxy;
    private SubmarineOxygen subOxy;
    private Submarine submarine;
    private Collider2D oxyCollider;
    private Rigidbody2D playerRB;
    private Rigidbody2D subRB;

    void Start()
    {
        playerAirProbe = player.gameObject.GetComponentInChildren<Collider2D>();
        playerOxy = player.GetComponent<PlayerOxygen>();
        playerRB = player.GetComponent<Rigidbody2D>();
        subOxy = sub.GetComponent<SubmarineOxygen>();
        submarine = sub.GetComponent<Submarine>();
        oxyCollider = GetComponent<Collider2D>();
        subRB = sub.GetComponent<Rigidbody2D>();

        if (playerAirProbe.IsTouching(oxyCollider))
            playerOxy.EnterOxygenZone();
        else
            playerOxy.ExitOxygenZone();

        if (subAirProbe.IsTouching(oxyCollider))
            subOxy.EnterOxygenZone();
        else
            subOxy.ExitOxygenZone();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == playerAirProbe)
        {
            playerOxy.EnterOxygenZone();
            playerRB.gravityScale = 1f;
        }

        if (other == subAirProbe)
        {
            subOxy.EnterOxygenZone();
            subRB.gravityScale = 1f;
        }

        //special case for when submarine runs out of air, player oxy starts dropping, and nothing resets playerOxy in the event of a submarine resurfacing
        if (other == subAirProbe && submarine.PlayerInside) 
            playerOxy.EnterOxygenZone();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == playerAirProbe)
        {
            playerOxy.ExitOxygenZone();
            playerRB.gravityScale = 0f;
        }

        if (other == subAirProbe)
        {
            subOxy.ExitOxygenZone();
            subRB.gravityScale = 0f;
        }
    }

}
