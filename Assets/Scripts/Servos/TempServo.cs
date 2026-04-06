using UnityEngine;
using System.Collections;

public class TempServo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private SubmarineTemp submarineTemp;
    [SerializeField] private float updateInterval = 1f;

    private Coroutine servoRoutine;

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
    }

    private IEnumerator ServoTick()
    {
        while (true)
        {
            float t = Mathf.Clamp01(submarineTemp.CurrentTemp / submarineTemp.MaxTemp);
            int angle = Mathf.RoundToInt(Mathf.Lerp(0f, 180f, t));
            SerialHandler.Instance.SendSerialData($"$TEMP:{angle}");

            yield return new WaitForSeconds(updateInterval);
        }
    }
}
