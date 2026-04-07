using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SubmarineTemp : MonoBehaviour
{
    [SerializeField] private float currentTemp;
    [SerializeField] private float maxTemp = 1000f;

    [Header("OverHeatings")]
    private bool overheating = false;
    private Coroutine overheatRoutine = null;
    private Coroutine coolantRoutine = null;

    [Header("Coolant Tank")]
    [SerializeField] private float currentCoolant;
    [SerializeField] private float maxCoolant = 5000f;
    [SerializeField] private float coolantStrength = 0.15f;   // % of temp removed per sec at full flow
    [SerializeField] private float coolantDrainRate = 50f;     // units of coolant consumed per sec at full flow

    [Header("Passive Cooling")]
    [SerializeField] private float passiveCoolStrength = 0.02f; // 2% per sec

    private float heatRate = 0f;         // heat added per sec (from lights, etc.)
    private float coolantFlow = 0f;      // 0 = off, 1 = full (from pot)
    private bool tickRunning = false;

    public float CurrentTemp => currentTemp;
    public float MaxTemp => maxTemp;
    public float HeatRate => heatRate;
    public float CurrentCoolant => currentCoolant;
    public float MaxCoolant => maxCoolant;

    [Header("Events")]
    public UnityEvent<float, float> tempChanged;
    public UnityEvent<float> heatRateChanged;
    public UnityEvent<float, float> coolantChanged;
    public UnityEvent tempMaxReached;
    public UnityEvent<float> coolantFlowChanged;

    [Header("Overheat Damage")]
    [SerializeField] private float overheatInterval = 1f;
    [SerializeField] private float overheatDamage = 5f;
    [SerializeField] private Health subHealth;


    Coroutine tempTick = null;

    private void Start()
    {
        StartCoroutine(DebugLog());
    }

    private IEnumerator DebugLog()
    {
        while (true)
        {
            Debug.Log($"[SubTemp] Temp: {currentTemp:F1}/{maxTemp} | HeatRate: {heatRate:F1} | Coolant: {currentCoolant:F1}/{maxCoolant} | Flow: {coolantFlow:F2}");
            yield return new WaitForSeconds(1f);
        }
    }
    private void Awake()
    {
        currentCoolant = maxCoolant;
        coolantChanged?.Invoke(currentCoolant, maxCoolant);
    }

    private void Update()
    {
        if (SerialHandler.Instance != null)
        {
            // Debug.Log("coolantPot: " + SerialHandler.Instance.coolantPot);
            SetCoolantFlow(1f - SerialHandler.Instance.coolantPot);
        }
    }

    public void AddFlatHeat(float heat)
    {
        currentTemp += heat; 
        tempChanged?.Invoke(currentTemp, maxTemp);
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

    public void SetCoolantFlow(float flow01)
    {
        coolantFlow = Mathf.Clamp01(flow01);
        coolantFlowChanged?.Invoke(coolantFlow);
        TryStartTick();
    }

    private void TryStartTick()
    {
        if (!tickRunning)
        {
            tickRunning = true;
            tempTick = StartCoroutine(TempTick());
        }
    }

    private IEnumerator TempTick()
    {
        while (true)
        {
            float dt = Time.deltaTime;

            // --- Heating ---
            if (heatRate > 0f)
                currentTemp += heatRate * dt;

            // --- Coolant (scaled by pot) ---
            if (coolantFlow > 0f && currentCoolant > 0f && currentTemp > 0f)
            {
                float cooling = currentTemp * coolantStrength * coolantFlow * dt;
                currentTemp -= cooling;

                currentCoolant = Mathf.Max(0f, currentCoolant - coolantDrainRate * coolantFlow * dt);
                // Debug.Log(currentCoolant); 
                coolantChanged?.Invoke(currentCoolant, maxCoolant);


                if (currentCoolant <= 0f)
                    coolantFlow = 0f;
            }

            // --- Passive cooling (percentage-based, no tank) ---
            if (heatRate == 0f)
            {
                currentTemp -= currentTemp * passiveCoolStrength * dt;
            }

            currentTemp = Mathf.Clamp(currentTemp, 0f, maxTemp);
            tempChanged?.Invoke(currentTemp, maxTemp);

            if (currentTemp >= maxTemp && !overheating)
            {
                overheating = true;
                overheatRoutine = StartCoroutine(OverheatDamage());
            }
            else if (currentTemp < maxTemp && overheatRoutine != null)
            {
                StopCoroutine(overheatRoutine);
                overheatRoutine = null;
                overheating = false;
            }

            // Nothing happening — exit
            if (heatRate <= 0f && coolantFlow <= 0f && currentTemp <= 0f)
                break;

            yield return null;
        }
        tempTick = null;
        tickRunning = false;
    }
    public void AddCoolant(float amount)
    {
        currentCoolant = Mathf.Clamp(currentCoolant + amount, 0f, maxCoolant);
        coolantChanged?.Invoke(currentCoolant, maxCoolant);
    }

    public void ResetCoolant()
    {
        currentCoolant = maxCoolant;
        coolantChanged?.Invoke(currentCoolant, maxCoolant);
    }

    private IEnumerator OverheatDamage()
    {
        while (overheating)
        {
            if (subHealth != null && !subHealth.isDead)
                subHealth.TakeDamage(overheatDamage);

            yield return new WaitForSeconds(overheatInterval);
        }
        overheatRoutine = null;
    }


}