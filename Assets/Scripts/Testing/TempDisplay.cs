using UnityEngine;
using TMPro;

public class TempDisplay : MonoBehaviour
{
    [SerializeField] private SubmarineTemp temp;
    [SerializeField] private TMP_Text tempText;

    private void OnEnable()
    {
        UpdateTemp(temp.CurrentTemp, temp.MaxTemp);
        temp.tempChanged.AddListener(UpdateTemp);
    }

    private void OnDisable()
    {
        temp.tempChanged.RemoveListener(UpdateTemp);
    }

    private void UpdateTemp(float currentTemp, float maxTemp)
    {
        tempText.text = Mathf.Round(currentTemp) + " / " + maxTemp;
    }
}