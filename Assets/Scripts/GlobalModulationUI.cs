using UnityEngine;

public class GlobalModulationUI : MonoBehaviour
{
    public static GlobalModulationUI Instance { get; private set; } // singleton

    [Header("UI References")]
    public GameObject minigameRoot;    
    public GameObject[] channelWarningOverlays = new GameObject[4]; 
    public RectTransform[] playerMarkers = new RectTransform[4];

    [Header("Layout")]
    public float sliderHalfWidth = 150f;
    public float rotaryMinDeg = -135f;
    public float rotaryMaxDeg = 135f;

    private void Awake()
    {
        //singleton logic, only ever ensure 1 instance of this obj.
        if (Instance != null && Instance != this) { Destroy(this.gameObject); return; }
        Instance = this;
        
        if (minigameRoot != null) minigameRoot.SetActive(false);
    }

    public void ActivateUI()
    {
        minigameRoot.SetActive(true);
        
        foreach(var overlay in channelWarningOverlays) 
        {
            if (overlay != null) overlay.SetActive(true);
        }
    }

    public void DeactivateUI() => minigameRoot.SetActive(false);

    //called by modulation minigame Update()
    public void UpdateVisuals(float[] currentValues, bool[] inRange, int[] modes)
    {
        for (int i = 0; i < 4; i++)
        {
            //update the controls wrt serial Handler inputs 
            ApplyMarker(playerMarkers[i], currentValues[i], modes[i]);
            
            //note that we set these active for as long as the player has NOT gotten the target, it is an overlayed gameobject that hides the SUCCESS UI below it. 
            if (channelWarningOverlays[i] != null)
            {
                channelWarningOverlays[i].SetActive(!inRange[i]);
            }
        }
    }

    //sets the rotary and slider markers to match, called from within UpdateVisuals() which is called from ModulationMinigame
    private void ApplyMarker(RectTransform rt, float value, int mode)
    {
        if (rt == null) return;

        if (mode == 0) //rotary
        {
            rt.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(rotaryMinDeg, rotaryMaxDeg, value));
        }
        else //sliders (1 and 2)
        {
            Vector2 pos = rt.anchoredPosition;
            if (mode == 1)
            {
                pos.x = Mathf.Lerp(-sliderHalfWidth, sliderHalfWidth, value);
            }
            else
            {
                pos.y = Mathf.Lerp(-sliderHalfWidth, sliderHalfWidth, value);
                rt.anchoredPosition = pos;
            }           
        }
    }
}