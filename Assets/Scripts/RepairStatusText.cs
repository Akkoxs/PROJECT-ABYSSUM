using UnityEngine;
using TMPro; // Required for TextMeshPro

// Monitors a list of GameObjects and updates a TMP text component
// to show a master RED "REPAIR" or GREEN "OK" status.
public class RepairStatusText : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The TextMeshPro text component to update.")]
    public TextMeshProUGUI statusText;

    [Tooltip("Reference to the warning light controller managing the leak cycles.")]
    public WarningLightController warningLightController;

    [Header("Status Settings")]
    public Color nominalColor = Color.green;
    public Color repairColor = Color.red;

    void Update()
    {
        if (warningLightController == null) return;

        // Query the data layer instead of the flashing GameObjects
        bool isFaulty = warningLightController.IsAnyChannelFlashing();

        // Apply the visual changes based on the fault state
        if (isFaulty)
        {
            if (statusText.text != "REPAIR") // Only update on exact state changes
            {
                statusText.text = "REPAIR";
                statusText.color = repairColor;
            }
        }
        else
        {
            if (statusText.text != "NOMINAL")
            {
                statusText.text = "NOMINAL";
                statusText.color = nominalColor;
            }
        }
    }
}