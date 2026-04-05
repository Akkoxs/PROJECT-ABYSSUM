using UnityEngine;

public class TempGaugeTicker : MonoBehaviour
{
    [SerializeField] private SubmarineTemp submarineTemp;
    [SerializeField] private float minAngle = 0f;
    [SerializeField] private float maxAngle = -350f;

    private void OnEnable()
    {
        submarineTemp.tempChanged.AddListener(UpdateGauge);
    }

    private void OnDisable()
    {
        submarineTemp.tempChanged.RemoveListener(UpdateGauge);
    }

    private void UpdateGauge(float current, float max)
    {
        float t = Mathf.Clamp01(current / max);
        float angle = Mathf.Lerp(minAngle, maxAngle, t);
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}