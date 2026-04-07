using TMPro;
using UnityEngine;

public class SubmarineHealthText : MonoBehaviour
{
    [SerializeField] private Health subHealth; 
    [SerializeField] private TMP_Text text;

    private void Update()
    {
        text.text = $"[HULL]:{Mathf.RoundToInt(subHealth.CurrentHealth)}/{Mathf.RoundToInt(subHealth.MaxHealth)}";
    }
}
