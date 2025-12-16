using UnityEngine;

public class HealBox : MonoBehaviour
{

    //Goes like "yo, thing I just hit, do you have a Healable interface?"
    //so long as your interface is not null, heal you 10 points. 

    private void OnCollisionEnter2D(Collision2D other)
    {
        IHealable healable = other.gameObject.GetComponent<IHealable>(); 
        if(healable != null) 
            healable.Heal(10f); 
    }
}
