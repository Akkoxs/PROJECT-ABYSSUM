using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;

public class LightingSystem : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private SubmarineTemp submarineTemp;
    [SerializeField] private TMP_Text headPercentText;
    [SerializeField] private TMP_Text floodPercentText;

    [Header("Headlight")]
    [SerializeField] private Light2D headLight;
    [SerializeField] private float headMaxIntensity = 80f;
    [SerializeField] private float headHeatRate = 0.005f; //per tick 

    [Header("Floodlights")]
    [SerializeField] private Light2D[] floodLights = new Light2D[2];
    [SerializeField] private float floodMaxIntensity = 80f;
    [SerializeField] private float floodHeatRate = 0.005f; //per tick 

    private float headHeat;
    private float floodHeat;

    void Update()
    {
        //get the newest serial vals from the serial handler singleton (between 0 and 1)
        float head = SerialHandler.Instance.headSlider;
        float flood = SerialHandler.Instance.floodSlider;

        //set headlight intenstiy 
        headLight.intensity = head * headMaxIntensity;

        //for all of the floodlights (2), set the intensity of them 
        foreach (var light in floodLights)
        {
            light.intensity = flood * floodMaxIntensity;
        } 

        //set the heats of the light (this also checks if they changed)
        SetHeat(ref headHeat,  head  * headHeatRate);
        SetHeat(ref floodHeat, flood * floodHeatRate * floodLights.Length);

        //set the current intensity values to the Ui 
        headPercentText.text  = $"{Mathf.RoundToInt(head  * 100f)}%";
        floodPercentText.text = $"{Mathf.RoundToInt(flood * 100f)}%";
    }

    //pass a current heat rate and a target heat rate 
    //ref is apparently like accessing the pointer of the 1st arg, its not local to the method
    void SetHeat(ref float stored, float target)
    {
        float rounded = Mathf.Round(target * 10000f) / 10000f;
        if (Mathf.Approximately(stored, rounded)) return; //exit early if there is no change in heat rate 
        submarineTemp.RemoveHeatSource(stored); //remove the previously applied heat contribution
        submarineTemp.AddHeatSource(target); //and add the new contribution
        stored = target; //the stored value becomes the current value 
    }
}