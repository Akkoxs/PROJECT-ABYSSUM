using UnityEngine;
using UnityEngine.Events;
using System.Collections;


public class SubmarineOxygen : MonoBehaviour
{
    [SerializeField] private float currentOxygen; 
    [SerializeField] private float maxOxygen = 750f;
    [SerializeField] private float depletionRate = 100f; //ticks per second
    [SerializeField] private PlayerOxygen playerOxy;
    [SerializeField] private Submarine sub;
    private bool isUnderwater;
    private bool hasInvokedDepletion;

    //public read only vars
    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;

    //Events 
    public UnityEvent <float, float> oxygenChanged;
    public UnityEvent oxygenDepleted;
    
    Coroutine oxyTick = null;

    private void Awake()
    {
        currentOxygen = maxOxygen;
        sub.enteredSubmarine.AddListener(PlayerEnteredSubmarine);
        sub.exitedSubmarine.AddListener(PlayerExitedSubmarine);

    }

    //triggered by OxygenZone
    public void EnterOxygenZone() //When sub surfaces with player inside
    {
        isUnderwater = false;
        ResetOxygen();

        if(oxyTick != null && sub.PlayerInside)
        {
            StopCoroutine(oxyTick);
            oxyTick = null;
        }
    }

    //triggered by OxygenZone
    public void ExitOxygenZone() //When sub submerges with player inside
    {
        isUnderwater = true;

        if (oxyTick == null && sub.PlayerInside)
            oxyTick = StartCoroutine(OxygenTick());
    }

    private void PlayerEnteredSubmarine() //when player exits sub while underwater
    {
        if (isUnderwater && oxyTick == null)
            oxyTick = StartCoroutine(OxygenTick());
    }

    private void PlayerExitedSubmarine() //when player exits sub while underwater
    {
        if (isUnderwater && oxyTick != null)
        {
            StopCoroutine(oxyTick);
            oxyTick = null;
        }
    }

    public void ResetOxygen()
    {
        currentOxygen = maxOxygen;
        oxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    private IEnumerator OxygenTick()
    {
        while (isUnderwater && sub.PlayerInside)
        {
            if (currentOxygen > 0f)
            {
                currentOxygen = Mathf.Max(0, currentOxygen - depletionRate * Time.deltaTime);
                oxygenChanged?.Invoke(currentOxygen, maxOxygen);
            }

            else if (currentOxygen <= 0)
            {
                oxygenDepleted?.Invoke();
                yield return new WaitForSeconds(1f);
            }
            yield return null; 
        }
    }
}
