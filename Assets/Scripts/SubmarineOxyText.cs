using UnityEngine;
using TMPro;

public class SubmarineOxyText : MonoBehaviour
{
    [SerializeField] private SubmarineOxygen subOxy; 
    [SerializeField] private TMP_Text text;

    private void Update()
    {
        text.text = $"[OXY]:{Mathf.RoundToInt(subOxy.CurrentOxygen)}/{Mathf.RoundToInt(subOxy.MaxOxygen)}";
    }
}
