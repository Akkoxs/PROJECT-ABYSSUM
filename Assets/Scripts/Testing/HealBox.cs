using UnityEngine;

public class HealBox : MonoBehaviour, IRadarDetectable
{

    //Goes like "hey thing I just hit, do you have a Healable interface?"
    //so long as your interface is not null, heal you 10 points. 

    [SerializeField] GameObject sub;
    private string radarSignature = "HP+";

    private void OnCollisionEnter2D(Collision2D other)
    {
        IHealable playerHealable = other.gameObject.GetComponent<IHealable>(); 
        IHealable subHealable = sub.gameObject.GetComponent<IHealable>();

        if(playerHealable != null) 
            playerHealable.Heal(10f); 

        if (subHealable != null)
            subHealable.Heal(50f);
    }


    //For testing purposes, Healbox will be an artifact
    public string GetRadarDisplayName()
    {
        return radarSignature;
    }

    public RadarObjectType GetObjectType()
    {
        return RadarObjectType.Artifact;
    } 
}
