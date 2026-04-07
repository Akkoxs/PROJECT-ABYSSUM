using UnityEngine;

public class CoolantGaugeTicker : MonoBehaviour
{
    [SerializeField] private SubmarineTemp submarineTemp;
    [SerializeField] private float minAngle = 0f;
    [SerializeField] private float maxAngle = 45f;

    private void OnEnable()
    {
        submarineTemp.coolantFlowChanged.AddListener(UpdateGauge);
        UpdateGauge(0f);
    }

    private void OnDisable()
    {
        submarineTemp.coolantFlowChanged.RemoveListener(UpdateGauge);
    }

    private void UpdateGauge(float flow01)
    {
        float angle = Mathf.Lerp(minAngle, maxAngle, flow01);
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}