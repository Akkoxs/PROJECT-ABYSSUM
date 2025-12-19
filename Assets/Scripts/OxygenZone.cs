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

    void Start()
    {
        playerAirProbe = player.gameObject.GetComponentInChildren<Collider2D>();
        playerOxy = player.GetComponent<PlayerOxygen>();
        subOxy = sub.GetComponent<SubmarineOxygen>();
        submarine = sub.GetComponent<Submarine>();
        oxyCollider = GetComponent<Collider2D>();


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
            playerOxy.EnterOxygenZone();

        if (other == subAirProbe)
            subOxy.EnterOxygenZone();

        //special case for when submarine runs out of air, player oxy starts dropping, and nothing resets playerOxy in the event of a submarine resurfacing
        if (other == subAirProbe && submarine.PlayerInside) 
            playerOxy.EnterOxygenZone();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == playerAirProbe)
            playerOxy.ExitOxygenZone();

        if (other == subAirProbe)
            subOxy.ExitOxygenZone();
    }

}
