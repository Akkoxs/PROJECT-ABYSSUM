using UnityEngine;
using UnityEngine.Events;
using System.Collections;


public class Oxygen : MonoBehaviour
{

    public UnityEvent resetOxygen;

    private void Awake()
    {
        //Add ResetOxygen() as a listener of a EnteredOxygen Event (that is invoked at submarine & OxygenZone classes)
    }

    public void EnterWater()
    {
        
    }


    public void ExitWater()
    {
        
    }


    void Update()
    {
        
        //Run OxygenTick() -> Oxygen always decreasing 
        //Run Oxygen Depleted when we run out of oxygen
        //Check to see if we go into a zone that allows us to ResetOxygen and does it. 
    }

    public void ResetOxygen()
    {
        //Invokes ResetOxygen event to let UI know
    }

    private IEnumerator OxygenTick()
    {
        //Decreases oxygen incrementally for as long as you are underwater
        yield return new WaitForSeconds(1f);
    }


    private IEnumerator OxygenDepletedDamage()
    {
        //Decreases health incrementally by using IDamageable TakeDamage() on player when we have run out of Oxygen & we are underwater
        yield return new WaitForSeconds(1f);
    }

}
