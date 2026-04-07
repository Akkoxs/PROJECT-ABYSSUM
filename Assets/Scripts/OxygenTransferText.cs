using TMPro;
using UnityEngine;

public class OxygenTransferText : MonoBehaviour
{

    [SerializeField] private PlayerOxygen playerOxy; 
    [SerializeField] private TMP_Text text;

private void Update()
    {
        text.text = $"DIVER TANK {Mathf.RoundToInt((playerOxy.CurrentOxygen / playerOxy.MaxOxygen)*100f)}%";
    }

}
