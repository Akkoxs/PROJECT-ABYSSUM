using UnityEngine;
using UnityEngine.Events;
using System.Collections;


public class PlayerOxygen : MonoBehaviour
{
    [SerializeField] private float currentOxygen; 
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float depletionRate = 0.5f; //ticks per second
    [SerializeField] private float depletionTickDamage = 5f; //per second 
    [SerializeField] private float depletionTickRate = 1f; 
    [SerializeField] private GameObject submarine;
    private bool isUnderwater;

    private Health playerHealth;
    private Submarine sub;
    private SubmarineOxygen subOxy;

    //public vars
    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;

    //Events 
    public UnityEvent <float, float> oxygenChanged;
    public UnityEvent oxygenDepleted;

    Coroutine oxyTick = null;
    Coroutine oxyDepleted = null;

    private void Awake()
    {
        currentOxygen = maxOxygen;
        playerHealth = GetComponent<Health>();
        sub = submarine.GetComponent<Submarine>();
        subOxy = submarine.GetComponent<SubmarineOxygen>();
    }

    private void Start()
    {
        sub.enteredSubmarine.AddListener(EnterSubmarine);
        sub.exitedSubmarine.AddListener(ExitSubmarine);
        subOxy.oxygenDepleted.AddListener(SubmarineLinkLogic);
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

    //Entering submarine when it has no oxygen is treated the same as being underwater
    public void EnterSubmarine() 
    {
        if (subOxy.CurrentOxygen > 0)
            EnterOxygenZone();
        else   
            ExitOxygenZone();
    }

    public void ExitSubmarine()
    {
        ExitOxygenZone();
    }

    private void SubmarineLinkLogic() //if the submarine depletes of oxygen while we are inside, it floods with water
    {
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
        oxyDepleted = null;
    }

    public void SetMaxOxygen(float newMax)
    {
        maxOxygen = newMax; 
        currentOxygen = Mathf.Min(currentOxygen, maxOxygen); //clamps
        oxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

}
