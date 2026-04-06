using UnityEngine;
using UnityEngine.UI;

public class TemperatureBarSubmarine : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private SubmarineTemp temp;
    [SerializeField] private UIHelper uiHelper;
    [SerializeField] private Image barFill;
    [SerializeField] private Image flashFill;
    [SerializeField] private float flashDuration = 0.2f;

    private Color flashColor = Color.white;
    private Color regColor;

    private void OnEnable()
    {
        flashFill.color = regColor;
        UpdateTemp(temp.CurrentTemp, temp.MaxTemp);
        temp.tempChanged.AddListener(UpdateTemp);
        temp.tempMaxReached.AddListener(TempMaxReached);
    }

    private void OnDisable()
    {
        temp.tempChanged.RemoveListener(UpdateTemp);
        temp.tempMaxReached.RemoveListener(TempMaxReached);
    }

    private void UpdateTemp(float currentTemp, float maxTemp)
    {
        slider.value = currentTemp / maxTemp;
    }

    private void TempMaxReached()
    {
        StartCoroutine(uiHelper.BarFlash(flashDuration, flashColor, regColor, flashFill));
    }
}