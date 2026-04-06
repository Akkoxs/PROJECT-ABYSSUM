using UnityEngine;
using System.Collections;

public class CoolantServo : MonoBehaviour
{
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
            float t = Mathf.Clamp01(submarineTemp.CurrentCoolant / submarineTemp.MaxCoolant);
            int angle = Mathf.RoundToInt(Mathf.Lerp(0f, 180f, t));
            SerialHandler.Instance.SendSerialData($"$COOL:{angle}");

            yield return new WaitForSeconds(updateInterval);
        }
    }
}