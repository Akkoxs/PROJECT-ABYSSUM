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
    private bool isInsideSub; 

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
        subOxy.oxygenDepleted.AddListener(OnSubmarineOxygenDepleted);
    }

    public void EnterOxygenZone()
    {
        isUnderwater = false;
        StopAllOxyCoroutines();

        if (!isInsideSub)
            ResetOxygen();
    }

    public void ExitOxygenZone()
    {
        isUnderwater = true;

        if (!isInsideSub)
            StartOxyTick();
    }

    public void EnterSubmarine()
    {
        isInsideSub = true;
        // Stop draining — player is sheltered, but does NOT refill
        StopAllOxyCoroutines();
    }

    public void ExitSubmarine()
    {
        isInsideSub = false;
        // Back underwater and exposed
        if (isUnderwater)
            StartOxyTick();
    }

    //if sub runs out of oxygen while player is inside 
    private void OnSubmarineOxygenDepleted()
    {
        if (isInsideSub)
        {
            // Sub has flooded so we treat player as underwater
            isUnderwater = true;
            StartOxyTick();
        }
    }

    //called by oxygen transfer system 
    public void ReceiveOxygen(float amount)
    {
        currentOxygen = Mathf.Min(currentOxygen + amount, maxOxygen);
        oxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    public void ResetOxygen()
    {
        currentOxygen = maxOxygen;
        oxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    //called by stat setter i believe
    public void SetMaxOxygen(float newMax)
    {
        maxOxygen = newMax;
        currentOxygen = Mathf.Min(currentOxygen, maxOxygen);
        oxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    //HELPERS -----------
    private void StartOxyTick()
    {
        if (oxyTick == null)
            oxyTick = StartCoroutine(OxygenTick());
    }

    private void StopAllOxyCoroutines()
    {
        if (oxyTick != null)
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

    private IEnumerator OxygenTick()
    {
        while (isUnderwater && !isInsideSub)
        {
            if (currentOxygen > 0f)
            {
                currentOxygen = Mathf.Max(0f, currentOxygen - depletionRate * Time.deltaTime);
                oxygenChanged?.Invoke(currentOxygen, maxOxygen);
            }

            if (currentOxygen <= 0f && oxyDepleted == null)
                oxyDepleted = StartCoroutine(OxygenDepletedDamage());

            yield return null;
        }
    }

    private IEnumerator OxygenDepletedDamage()
    {
        while (isUnderwater && !isInsideSub && currentOxygen <= 0f)
        {
            oxygenDepleted?.Invoke();
            if (playerHealth != null && !playerHealth.isDead)
                playerHealth.TakeDamage(depletionTickDamage);

            yield return new WaitForSeconds(depletionTickRate);
        }
        oxyDepleted = null;
    }
}
