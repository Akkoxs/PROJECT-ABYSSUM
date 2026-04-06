using UnityEngine;
using UnityEngine.UI;

public class CoolantBar : MonoBehaviour
{
    [SerializeField] private SubmarineTemp submarineTemp;
    private Slider coolantSlider;

    private void Awake()
    {
        coolantSlider = GetComponent<Slider>();
        coolantSlider.minValue = 0f;
        coolantSlider.maxValue = 1f;
        coolantSlider.interactable = false;
    }

    private void OnEnable()
    {
        submarineTemp.coolantChanged.AddListener(OnCoolantChanged);
    }

    private void OnDisable()
    {
        submarineTemp.coolantChanged.RemoveListener(OnCoolantChanged);
    }

    private void OnCoolantChanged(float current, float max)
    {
        coolantSlider.value = (max > 0f) ? current / max : 0f;
    }
}