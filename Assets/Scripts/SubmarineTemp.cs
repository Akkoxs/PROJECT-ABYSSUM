using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SubmarineTemp : MonoBehaviour
{
    [SerializeField] private float currentTemp;
    [SerializeField] private float maxTemp = 1000f;

    [Header("Coolant Tank")]
    [SerializeField] private float currentCoolant;
    [SerializeField] private float maxCoolant = 5000f;
    [SerializeField] private float coolantStrength = 0.15f;   // % of temp removed per sec
    [SerializeField] private float coolantDrainRate = 50f;     // units of coolant consumed per sec

    [Header("Passive Cooling")]
    [SerializeField] private float passiveCoolStrength = 0.02f; // 2% per sec

    private float heatRate = 0f;         // heat added per sec (from lights, etc.)
    private bool coolantActive = false;

    public float CurrentTemp => currentTemp;
    public float MaxTemp => maxTemp;
    public float HeatRate => heatRate;
    public float CurrentCoolant => currentCoolant;
    public float MaxCoolant => maxCoolant;

    public UnityEvent<float, float> tempChanged;
    public UnityEvent<float> heatRateChanged;
    public UnityEvent<float, float> coolantChanged;
    public UnityEvent tempMaxReached;

    Coroutine tempTick = null;

    private void Awake()
    {
        currentTemp = 0f;
        currentCoolant = maxCoolant;
        // Notify listeners of the initial coolant state so UI shows full tank immediately
        coolantChanged?.Invoke(currentCoolant, maxCoolant);
    }

    public void AddHeatSource(float rate)
    {
        heatRate += rate;
        heatRateChanged?.Invoke(heatRate);
        TryStartTick();
    }

    public void RemoveHeatSource(float rate)
    {
        heatRate = Mathf.Max(0f, heatRate - rate);
        heatRateChanged?.Invoke(heatRate);
    }

    public void SetCoolantActive(bool active)
    {
        coolantActive = active;
        TryStartTick();
    }

    private void TryStartTick()
    {
        if (tempTick == null)
            tempTick = StartCoroutine(TempTick());
    }

    private IEnumerator TempTick()
    {
        while (true)
        {
            float dt = Time.deltaTime;

            // --- Heating ---
            if (heatRate > 0f)
                currentTemp += heatRate * dt;

            // --- Coolant (percentage-based, drains tank) ---
            if (coolantActive && currentCoolant > 0f && currentTemp > 0f)
            {
                float cooling = currentTemp * coolantStrength * dt;
                currentTemp -= cooling;

                currentCoolant = Mathf.Max(0f, currentCoolant - coolantDrainRate * dt);
                coolantChanged?.Invoke(currentCoolant, maxCoolant);

                if (currentCoolant <= 0f)
                    coolantActive = false;
            }

            // --- Passive cooling (percentage-based, no tank) ---
            if (heatRate == 0f) 
            {
                currentTemp -= currentTemp * passiveCoolStrength * dt;
            }

            currentTemp = Mathf.Clamp(currentTemp, 0f, maxTemp);
            tempChanged?.Invoke(currentTemp, maxTemp);

            if (currentTemp >= maxTemp)
            {
                tempMaxReached?.Invoke();
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Nothing happening — exit
            if (heatRate <= 0f && !coolantActive && currentTemp <= 0f)
                break;

            yield return null;
        }

        tempTick = null;
    }
    // [SerializeField] private float currentTemp;
    // [SerializeField] private float maxTemp = 1000f;
    // [SerializeField] private float maxCoolant = 5000f;
    // [SerializeField] private float currentCoolant;
    // [SerializeField] private Submarine sub;



    // public float heatRate = 0f;   // total heat being added per second (from lights, etc.)
    // private float coolantRate = 5f;   // total cooling per second (from pumps, etc.)
    // private float coolRate = 0f;   // passive cooling rate when no heat sources are active
    // // Public read-only vars
    // public float CurrentTemp => currentTemp;
    // public float MaxTemp => maxTemp;
    // public float HeatRate => heatRate;
    // public float CurrentCoolant => currentCoolant; 

    // // Events
    // public UnityEvent<float, float> tempChanged;
    // public UnityEvent<float> heatRateChanged;
    // public UnityEvent tempMaxReached;

    // Coroutine tempTick = null;

    // private void Awake()
    // {
    //     currentTemp = 0f;
    // }

    // // Called by heat sources (lights, etc.) when they turn on/off
    // public void AddHeatSource(float rate)
    // {
    //     Debug.Log("AddHeat Sent: " + rate);
    //     heatRate += rate;
    //     heatRateChanged?.Invoke(heatRate);
    //     TryStartTick();
    // }

    // public void RemoveHeatSource(float rate)
    // {
    //     heatRate = Mathf.Max(0f, heatRate - rate);
    //     heatRateChanged?.Invoke(heatRate);

    // }

    // // Called by cooling sources (pumps, etc.) when they turn on/off
    // public void AddCoolSource(float rate)
    // {
    //     coolRate += rate;
    //     TryStartTick();
    // }

    // public void RemoveCoolSource(float rate)
    // {
    //     coolRate = Mathf.Max(0f, coolRate - rate);
    // }

    // private void TryStartTick()
    // {
    //     if (tempTick == null)
    //         tempTick = StartCoroutine(TempTick());
    // }

    // private IEnumerator TempTick()
    // {
    //     while (true)
    //     {
    //         if (heatRate > 0f || coolRate > 0f)
    //         {
    //             float netChange = (heatRate - coolRate) * Time.deltaTime;
    //             currentTemp = Mathf.Clamp(currentTemp + netChange, 0f, maxTemp);
    //             currentCoolant = Mathf.Clamp(currentCoolant - coolantRate * Time.deltaTime, 0f, maxCoolant);
    //             tempChanged?.Invoke(currentTemp, maxTemp);

    //             if (currentTemp >= maxTemp)
    //             {
    //                 tempMaxReached?.Invoke();
    //                 yield return new WaitForSeconds(1f);
    //             }
    //         }
    //         else if (currentTemp > 0f && coolantRate > 0f)
    //         {
    //             float netChange = -coolantRate * Time.deltaTime;
    //             currentTemp = Mathf.Clamp(currentTemp + netChange, 0f, maxTemp);
    //             tempChanged?.Invoke(currentTemp, maxTemp);
    //         }
    //         else
    //         {
    //             // Nothing to do — stop the coroutine
    //             break;
    //         }

    //         yield return null;
    //     }

    //     tempTick = null;
    // }

    // public void SetMaxTemp(float newMax)
    // {
    //     maxTemp = newMax;
    //     currentTemp = Mathf.Min(currentTemp, maxTemp);
    //     tempChanged?.Invoke(currentTemp, maxTemp);
    // }
}