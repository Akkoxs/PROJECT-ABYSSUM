using UnityEngine;
using UnityEngine.Events;
using System.Collections;


public class SubmarineOxygen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Submarine sub;
    [SerializeField] private Health subHealth;

    [Header("Properties")]
    [SerializeField] private float currentOxygen; 
    [SerializeField] private float maxOxygen = 1000f;
    [SerializeField] private float depletionRate = 1f; //ticks per second
    [SerializeField] private float playerInsideMultiplier = 2f; //2x drain when player inside 
    [SerializeField] private float depletionTickDamage = 5f;
    [SerializeField] private float depletionTickRate = 1f;

    private bool isUnderwater;

    //public read only vars
    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;

    //Events 
    public UnityEvent <float, float> oxygenChanged;
    public UnityEvent oxygenDepleted;
    
    Coroutine oxyTick = null;
    Coroutine oxyDepletedDamage = null;

    private void Awake()
    {
        currentOxygen = maxOxygen;
    }

    //triggered by OxygenZone
    public void EnterOxygenZone()
    {
        isUnderwater = false;
        ResetOxygen();
        StopOxyTick();
        StopDamageTick();
    }

    //triggered by OxygenZone
    public void ExitOxygenZone() //When sub submerges with player inside
    {
        isUnderwater = true;
        StartOxyTick();
    }

    //to be called by oxygen transfer system
    public float RequestOxygen(float amount)
    {
        float given = Mathf.Min(amount, currentOxygen); //clamp so we dont seen below 0
        currentOxygen -= given; //subtract the given from our current reserves
        oxygenChanged?.Invoke(currentOxygen, maxOxygen); //let the event knon
        return given;
    }

    public void ResetOxygen()
    {
        currentOxygen = maxOxygen;
        oxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    //I believe this is called by the submarine stat setter
    public void SetMaxOxygen(float newMax)
    {
        maxOxygen = newMax; 
        currentOxygen = maxOxygen; //clamp
        oxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    //HELPERS -----------

    private void StartOxyTick()
    {
        if (oxyTick == null)
            oxyTick = StartCoroutine(OxygenTick());
    }

    private void StopOxyTick()
    {
        if (oxyTick != null)
        {
            StopCoroutine(oxyTick);
            oxyTick = null;
        }
    }

    private void StopDamageTick()
    {
        if (oxyDepletedDamage != null)
        {
            StopCoroutine(oxyDepletedDamage);
            oxyDepletedDamage = null;
        }
    }

    private IEnumerator OxygenTick()
    {
        while (isUnderwater)
        {
            float rate; 
            
            //how much do we deplete by? depends on if player inside or not 
            if (sub.PlayerInside)
            {
                rate = depletionRate * playerInsideMultiplier;
            }
            else
            {
                rate = depletionRate;
            }
            
            //as long as we have oxy
            if (currentOxygen > 0f)
            {
                currentOxygen = Mathf.Max(0f, currentOxygen - rate * Time.deltaTime); //subtract the current rate, but clamp so it doesnt go below 0
                oxygenChanged?.Invoke(currentOxygen, maxOxygen); //let the ui know

                // clear damage tick if we regained oxygen
                if (currentOxygen > 0f)
                    StopDamageTick();
            }

            //if we are out of oxy and not already taking damage, start the corot
            if (currentOxygen <= 0f && oxyDepletedDamage == null)
                oxyDepletedDamage = StartCoroutine(OxygenDepletedDamage());

            yield return null;
        }
    }

    private IEnumerator OxygenDepletedDamage()
    {
        while (isUnderwater && currentOxygen <= 0f)
        {
            oxygenDepleted?.Invoke();
            if (subHealth != null && !subHealth.isDead)
                subHealth.TakeDamage(depletionTickDamage);

            yield return new WaitForSeconds(depletionTickRate);
        }
        oxyDepletedDamage = null;
    }
}
