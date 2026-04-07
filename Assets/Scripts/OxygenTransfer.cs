using UnityEngine;

public class OxygenTransfer : MonoBehaviour
{

    [SerializeField] private SubmarineOxygen subOxy;
    [SerializeField] private PlayerOxygen playerOxy;
    [SerializeField] private Submarine sub; 

    [SerializeField] private float l1Amount = 5f;
    [SerializeField] private float l2Amount = 20f;
    [SerializeField] private float l3Amount = 50f;

    [Header("Button Indicator Lights")]
    [SerializeField] private GameObject l1Button;
    [SerializeField] private GameObject l2Button;
    [SerializeField] private GameObject l3Button;

    private bool l1WasPressed; 
    private bool l2WasPressed; 
    private bool l3WasPressed; 

    private void Update()
    {
        if (SerialHandler.Instance == null) return;

        bool l1 = SerialHandler.Instance.oxyL1;
        bool l2 = SerialHandler.Instance.oxyL2;
        bool l3 = SerialHandler.Instance.oxyL3;

        if (l1Button != null) l1Button.SetActive(!l1);
        if (l2Button != null) l2Button.SetActive(!l2);
        if (l3Button != null) l3Button.SetActive(!l3);

        if (!sub.PlayerInside) return;

        HandleTransfers(l1, ref l1WasPressed, l1Amount);
        HandleTransfers(l2, ref l2WasPressed, l2Amount);
        HandleTransfers(l3, ref l3WasPressed, l3Amount);
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
