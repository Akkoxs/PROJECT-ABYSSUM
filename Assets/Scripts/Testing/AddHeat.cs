using UnityEngine;

public class AddHeat : MonoBehaviour
{
    [SerializeField] private float heatRate = 20f;
    [SerializeField] private SubmarineTemp subTemp;
    private bool isOn = false;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (subTemp == null) return;

        isOn = !isOn;

        if (isOn)
            subTemp.AddHeatSource(heatRate);
        else
            subTemp.RemoveHeatSource(heatRate);
    }
}