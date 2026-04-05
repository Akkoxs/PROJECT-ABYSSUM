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
    [SerializeField] private float coolantStrength = 0.15f;   // % of temp removed per sec at full flow
    [SerializeField] private float coolantDrainRate = 50f;     // units of coolant consumed per sec at full flow

    [Header("Passive Cooling")]
    [SerializeField] private float passiveCoolStrength = 0.02f; // 2% per sec

    [Header("Arduino")]
    [SerializeField] private SerialController serialController;

    private float heatRate = 0f;         // heat added per sec (from lights, etc.)
    private float coolantFlow = 0f;      // 0 = off, 1 = full (from pot)

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
        currentCoolant = maxCoolant;
        coolantChanged?.Invoke(currentCoolant, maxCoolant);
    }

    private void Update()
    {
        if (SerialHandler.Instance != null)
        {
            Debug.Log("coolantPot: " + SerialHandler.Instance.coolantPot);
            SetCoolantFlow(SerialHandler.Instance.coolantPot);
        }
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

            // --- Coolant (scaled by pot) ---
            if (coolantFlow > 0f && currentCoolant > 0f && currentTemp > 0f)
            {
                float cooling = currentTemp * coolantStrength * coolantFlow * dt;
                currentTemp -= cooling;

                currentCoolant = Mathf.Max(0f, currentCoolant - coolantDrainRate * coolantFlow * dt);
                coolantChanged?.Invoke(currentCoolant, maxCoolant);

                // serialController.SendSerialMessage(currentCoolant.ToString("F0"));

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

            if (currentTemp >= maxTemp)
            {
                tempMaxReached?.Invoke();
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Nothing happening — exit
            if (heatRate <= 0f && coolantFlow <= 0f && currentTemp <= 0f)
                break;

            yield return null;
        }

        tempTick = null;
    }
}