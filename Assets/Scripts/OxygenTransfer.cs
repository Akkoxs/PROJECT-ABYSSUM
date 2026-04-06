using UnityEngine;

public class OxygenTransfer : MonoBehaviour
{

    [SerializeField] private SubmarineOxygen subOxy;
    [SerializeField] private PlayerOxygen playerOxy;
    [SerializeField] private Submarine sub; 

    [SerializeField] private float l1Amount = 5f;
    [SerializeField] private float l2Amount = 20f;
    [SerializeField] private float l3Amount = 50f;

    private bool l1WasPressed; 
    private bool l2WasPressed; 
    private bool l3WasPressed; 

    private void Update()
    {
        if (SerialHandler.Instance == null) return;

        if (!sub.PlayerInside) return;

        HandleTransfers(SerialHandler.Instance.oxyL1, ref l1WasPressed, l1Amount);
        HandleTransfers(SerialHandler.Instance.oxyL2, ref l2WasPressed, l2Amount);
        HandleTransfers(SerialHandler.Instance.oxyL3, ref l3WasPressed, l3Amount);
    }

    private void HandleTransfers(bool buttonDown, ref bool wasPressed, float amount)
    {
        if (buttonDown && !wasPressed)
        {
            wasPressed = true;
            float actuallyGiven = subOxy.RequestOxygen(amount);
            playerOxy.ReceiveOxygen(actuallyGiven);
        }
        else if (!buttonDown)
        {
            wasPressed = false;
        }
    }


}
