using UnityEngine;

public class GlobalModulationUI : MonoBehaviour
{
    public static GlobalModulationUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject minigameRoot;
    
    [Tooltip("These objects are ACTIVE when the player is WRONG, and turn OFF when they hit the target.")]
    public GameObject[] channelWarningOverlays = new GameObject[4]; 
    public RectTransform[] playerMarkers = new RectTransform[4];

    [Header("Gauge Layout")]
    public float sliderHalfWidth = 150f;
    public float rotaryMinDeg = -135f;
    public float rotaryMaxDeg = 135f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); return; }
        Instance = this;
        
        if (minigameRoot != null) minigameRoot.SetActive(false);
    }

    public void ActivateUI()
    {
        minigameRoot.SetActive(true);
        
        // Start with all "Warnings" VISIBLE because they haven't found the target yet
        foreach(var overlay in channelWarningOverlays) 
        {
            if (overlay != null) overlay.SetActive(true);
        }
    }

    public void DeactivateUI() => minigameRoot.SetActive(false);

    public void UpdateVisuals(float[] currentValues, bool[] inRange, int[] modes)
    {
        for (int i = 0; i < 4; i++)
        {
            // Move the player's needles/sliders
            ApplyMarker(playerMarkers[i], currentValues[i], modes[i]);
            
            // OPPOSITE LOGIC: 
            // If inRange is TRUE (Success), SetActive is FALSE (Hide Warning)
            // If inRange is FALSE (Error), SetActive is TRUE (Show Warning)
            if (channelWarningOverlays[i] != null)
            {
                channelWarningOverlays[i].SetActive(!inRange[i]);
            }
        }
    }

    private void ApplyMarker(RectTransform rt, float value, int mode)
    {
        if (rt == null) return;

        if (mode == 0) // Rotary
        {
            rt.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(rotaryMinDeg, rotaryMaxDeg, value));
        }
        else // Sliders
        {
            Vector2 pos = rt.anchoredPosition;
            if (mode == 1) pos.x = Mathf.Lerp(-sliderHalfWidth, sliderHalfWidth, value);
            else           pos.y = Mathf.Lerp(-sliderHalfWidth, sliderHalfWidth, value);
            rt.anchoredPosition = pos;
        }
    }
}