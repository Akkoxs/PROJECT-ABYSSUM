using UnityEngine;
using TMPro;

public class CoolantDisplay : MonoBehaviour
{
    [SerializeField] private SubmarineTemp temp;
    [SerializeField] private TMP_Text tempText;

    private void OnEnable()
    {
        temp.coolantChanged.AddListener(UpdateTemp);
        UpdateTemp(temp.CurrentCoolant, temp.MaxCoolant);
    }

    private void OnDisable()
    {
        temp.coolantChanged.RemoveListener(UpdateTemp);
    }

    private void UpdateTemp(float currentCoolant, float maxCoolant)
    {
        tempText.text = Mathf.Round(currentCoolant) + " / " + maxCoolant;
    }
}