using UnityEngine;
using System.Collections;

public class TempServo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private SubmarineTemp submarineTemp;
    [SerializeField] private float updateInterval = 1f;

    private Coroutine servoRoutine;

    private void Awake()
    {
        if (submarineTemp == null)
            submarineTemp = GetComponent<SubmarineTemp>();
    }

    private void OnEnable()
    {
        servoRoutine = StartCoroutine(ServoTick());
    }

    private void OnDisable()
    {
        if (servoRoutine != null)
        {
            StopCoroutine(servoRoutine);
            servoRoutine = null;
        }

        if (SerialHandler.Instance != null && SerialHandler.Instance.IsSerialReady)
        {
            SerialHandler.Instance.SendSerialData("TEMP:0");
        }
    }

    private void OnApplicationQuit()
    {
        if (SerialHandler.Instance != null && SerialHandler.Instance.IsSerialReady)
        {
            SerialHandler.Instance.SendSerialData("TEMP:0");
        }
    }

    private IEnumerator ServoTick()
    {
        yield return new WaitForSeconds(2f); // give serial time to connect
        while (true)
        {
            if (SerialHandler.Instance != null)
            {
                float t = Mathf.Clamp01(submarineTemp.CurrentTemp / submarineTemp.MaxTemp);
                int angle = Mathf.RoundToInt(Mathf.Lerp(180f, 0f, t));
                SerialHandler.Instance.SendSerialData($"TEMP:{angle}");
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }
}
