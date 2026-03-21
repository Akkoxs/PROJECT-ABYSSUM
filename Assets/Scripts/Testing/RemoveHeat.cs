using UnityEngine;

public class RemoveHeat : MonoBehaviour
{
    [SerializeField] private SubmarineTemp subTemp;
    private bool isOn = false;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (subTemp == null) return;
        isOn = !isOn;
        subTemp.SetCoolantActive(isOn);
    }
}