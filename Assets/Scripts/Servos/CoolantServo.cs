using UnityEngine;
using System.Collections;

public class CoolantServo : MonoBehaviour
{
    [SerializeField] private SubmarineTemp submarineTemp;
    [SerializeField] private float updateInterval = 1f;

    private Coroutine servoRoutine;

    private void Start()
    {
        submarineTemp = GetComponent<SubmarineTemp>();
        servoRoutine = StartCoroutine(ServoTick());
    }

    private void OnDisable()
    {
        if (servoRoutine != null)
        {
            StopCoroutine(servoRoutine);
            servoRoutine = null;
        }

        // SerialController may already be shut down, so guard before sending
        if (SerialHandler.Instance != null && SerialHandler.Instance.IsSerialReady)
        {
            SerialHandler.Instance.SendSerialData("COOL:180"); //this was given to me by claud as COOL:0 but idk 
        }
    }

    private void OnApplicationQuit()
    {
        if (SerialHandler.Instance != null && SerialHandler.Instance.IsSerialReady)
        {
            SerialHandler.Instance.SendSerialData("COOL:180"); //this was given to me by claud as COOL:0 but idk 
        }
    }

    private IEnumerator ServoTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); // give serial time to connect
            if (SerialHandler.Instance != null)
            {
                float t = Mathf.Clamp01(submarineTemp.CurrentCoolant / submarineTemp.MaxCoolant);
                int angle = Mathf.RoundToInt(Mathf.Lerp(0f, 180f, t));
                SerialHandler.Instance.SendSerialData($"COOL:{angle}");
                Debug.Log(angle);
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }
}