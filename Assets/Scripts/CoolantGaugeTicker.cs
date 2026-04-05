using UnityEngine;

public class CoolantGaugeTicker : MonoBehaviour
{
    [SerializeField] private SubmarineTemp submarineTemp;
    [SerializeField] private float minAngle = 0f;  // bottom-right (empty)
    [SerializeField] private float maxAngle = 45f;     // bottom-left (full)

    private void OnEnable()
    {
        submarineTemp.coolantChanged.AddListener(UpdateGauge);
        UpdateGauge(submarineTemp.CurrentCoolant, submarineTemp.MaxCoolant);
    }

    private void OnDisable()
    {
        submarineTemp.coolantChanged.RemoveListener(UpdateGauge);
    }

    private void UpdateGauge(float current, float max)
    {
        float t = Mathf.Clamp01(current / max);
        float angle = Mathf.Lerp(minAngle, maxAngle, t);
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}