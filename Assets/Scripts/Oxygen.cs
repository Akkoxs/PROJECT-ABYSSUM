using UnityEngine;
using UnityEngine.Events;
using System.Collections;


public class Oxygen : MonoBehaviour
{

    [SerializeField] private float currentOxygen; 
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float depletionRate = 0.5f; //ticks per second
    [SerializeField] private float depletionTickDamage = 5f; //per second 
    [SerializeField] private float depletionTickRate = 1f; 
    [SerializeField] private Health playerHealth;
    private bool isUnderwater;
    private bool inSubmarine;

    //public read only vars
    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;
    //public bool IsUnderwater => isUnderwater;

    //Events 
    public UnityEvent <float, float> oxygenChanged;
    public UnityEvent oxygenDepleted;
    public UnityEvent enteredSubmarine;
    public UnityEvent exitedSubmarine;


    Coroutine oxyTick = null;
    Coroutine oxyDepleted = null;

    private void Awake()
    {
        currentOxygen = maxOxygen;
    }


    public void EnterOxygenZone()
    {
        isUnderwater = false;
        ResetOxygen();

        if(oxyTick != null)
        {
            StopCoroutine(oxyTick);
            oxyTick = null;
        }

        if (oxyDepleted != null)
        {
            StopCoroutine(oxyDepleted);
            oxyDepleted = null;
        }
    }


    public void ExitOxygenZone()
    {
        isUnderwater = true;

        if (oxyTick == null)
            oxyTick = StartCoroutine(OxygenTick());
    }

    public void EnterSubmarine() //IN PROG
    {
        inSubmarine = true;
        //UI Oxybar = LINKED TO VESSEL and blank
        EnterOxygenZone();
    }

    public void ExitSubmarine() //IN PROG
    {
        inSubmarine = false;
        ExitOxygenZone();
    }

    public void ResetOxygen()
    {
        currentOxygen = maxOxygen;
        oxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    private IEnumerator OxygenTick()
    {
        while (isUnderwater)
        {
            if (currentOxygen > 0f)
            {
                currentOxygen = Mathf.Max(0, currentOxygen - depletionRate * Time.deltaTime);
                oxygenChanged?.Invoke(currentOxygen, maxOxygen);
            }

            else if (currentOxygen <= 0f && oxyDepleted == null)
                oxyDepleted = StartCoroutine(OxygenDepletedDamage());

            yield return null; 
        }
    }

    private IEnumerator OxygenDepletedDamage()
    {
        while (isUnderwater && currentOxygen <= 0)
        {
            if (playerHealth != null && !playerHealth.isDead)
            {
                oxygenDepleted?.Invoke(); 
                playerHealth.TakeDamage(depletionTickDamage);   
            }
           yield return new WaitForSeconds(depletionTickRate);
        }
    }

}
