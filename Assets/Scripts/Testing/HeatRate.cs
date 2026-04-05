using UnityEngine;
using TMPro;

public class HeatRate : MonoBehaviour
{
    [SerializeField] private SubmarineTemp temp;
    [SerializeField] private TMP_Text heatRateText;

    private void OnEnable()
    {
        UpdateHeatRate(temp.HeatRate);
        temp.heatRateChanged.AddListener(UpdateHeatRate);
    }

    private void OnDisable()
    {
        temp.heatRateChanged.RemoveListener(UpdateHeatRate);
    }

    private void UpdateHeatRate(float heatRate)
    {
        heatRateText.text = Mathf.Round(heatRate) + " heat/sec";
    }
}