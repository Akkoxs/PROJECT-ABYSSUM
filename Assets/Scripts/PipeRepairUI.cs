// using UnityEngine;

using UnityEngine;

// Visual half of the submarine pipe-burst repair mechanic, shown on the Diver Control quadrant
// (camRight / S4) in the same canvas region as the artifact modulation minigame UI.
public class PipeRepairUI : MonoBehaviour
{
    public static PipeRepairUI Instance { get; private set; } // singleton

    [Header("UI References")]
    public RectTransform[] channelMarkers = new RectTransform[4];
    
    [Tooltip("Reference to the script managing the blinking warning lights.")]
    public WarningLightController warningLightController;

    [Header("Audio")]
    [Tooltip("Ratchet/tick fired repeatedly while a burst control is being moved.")]
    [SerializeField] AudioClip repairMoveSFX;
    [Tooltip("Played once when a pipe/valve is fully repaired.")]
    [SerializeField] AudioClip repairDoneSFX;

    [Header("Marker Layout")]
    [Tooltip("Knob marker angle (degrees) when its control is at 0.")]
    public float rotaryMinDeg = -135f;
    [Tooltip("Knob marker angle (degrees) when its control is at 1.")]
    public float rotaryMaxDeg = 135f;
    
    [Tooltip("Lever marker angle (degrees) when its control is at 0.")]
    public float leverMinDeg = -80f;
    [Tooltip("Lever marker angle (degrees) when its control is at 1.")]
    public float leverMaxDeg = 80f;

    private void Awake()
    {
        //singleton logic, only ever ensure 1 instance of this obj.
        if (Instance != null && Instance != this) { Destroy(this.gameObject); return; }
        Instance = this;
    }

    public void PlayMoveSFX()
    {
        if (repairMoveSFX == null) return;
        AudioEventBus.RequestSFX(new SFXEvent(repairMoveSFX, volume: 1f, pitch: Random.Range(0.9f, 1.1f), pos: transform.position));
    }

    public void PlayRepairDoneSFX()
    {
        if (repairDoneSFX == null) return;
        AudioEventBus.RequestSFX(new SFXEvent(repairDoneSFX, volume: 1f, pitch: 1f, pos: transform.position));
    }

    //called every frame by PipeRepairSystem.Update()
    public void UpdateVisuals(float[] currentValues, bool[] burst, int[] modes)
    {
        for (int i = 0; i < 4; i++)
        {
            //move the marker to match the live control value
            ApplyMarker(channelMarkers[i], currentValues[i], modes[i]);

            // Pass the current burst state to the light controller
            if (warningLightController != null)
            {
                warningLightController.SetLightState(i, burst[i]);
            }
        }
    }

    private void ApplyMarker(RectTransform rt, float value, int mode)
    {
        if (rt == null) return;

        if (mode == 0) // Rotary Knobs
        {
            rt.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(rotaryMinDeg, rotaryMaxDeg, value));
        }
        else // Sliders acting as Levers (1 = horizontal, 2 = vertical)
        {
            rt.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(leverMinDeg, leverMaxDeg, value));
        }
    }
}



// // Visual half of the submarine pipe-burst repair mechanic, shown on the Diver Control quadrant
// // (camRight / S4) in the same canvas region as the artifact modulation minigame UI.
// //
// // Structurally this is the same shape as GlobalModulationUI (one toggled panel root, four channel
// // markers for the two knobs + two sliders, and four per-channel overlays) so the prefab is built
// // and wired the same way. The inspector fields are just named in pipe-repair terms:
// //   repairPanelRoot  <-> GlobalModulationUI.minigameRoot          (the toggled child panel)
// //   channelMarkers   <-> GlobalModulationUI.playerMarkers         (knob/slider indicators)
// //   leakOverlays     <-> GlobalModulationUI.channelWarningOverlays (the animated burst/leak art)
// public class PipeRepairUI : MonoBehaviour
// {
//     public static PipeRepairUI Instance { get; private set; } // singleton

//     [Header("Repair Panel")]
//     [Tooltip("Child panel that is shown while piloting and hidden during an artifact minigame.")]
//     //public GameObject repairPanelRoot;
//     //[Tooltip("The knob/slider indicator that tracks each control. Order: 0 knob, 1 h-slider, 2 knob, 3 v-slider.")]
//     public RectTransform[] channelMarkers = new RectTransform[4];
//     [Tooltip("Animated burst/leak art per channel. Shown while that pipe/valve is burst, hidden once repaired.")]
//     public GameObject[] leakOverlays = new GameObject[4];

//     [Header("Audio")]
//     [Tooltip("Ratchet/tick fired repeatedly while a burst control is being moved.")]
//     [SerializeField] AudioClip repairMoveSFX;
//     [Tooltip("Played once when a pipe/valve is fully repaired.")]
//     [SerializeField] AudioClip repairDoneSFX;

//     [Header("Marker Layout")]
//     [Tooltip("Half the travel of a slider marker, in anchored units (matches the slider track half-width).")]
//     public float sliderHalfWidth = 150f;
//     [Tooltip("Knob marker angle (degrees) when its control is at 0.")]
//     public float rotaryMinDeg = -135f;
//     [Tooltip("Knob marker angle (degrees) when its control is at 1.")]
//     public float rotaryMaxDeg = 135f;

//     private void Awake()
//     {
//         //singleton logic, only ever ensure 1 instance of this obj.
//         if (Instance != null && Instance != this) { Destroy(this.gameObject); return; }
//         Instance = this;

//         //if (repairPanelRoot != null) repairPanelRoot.SetActive(false);
//     }

//     // public void ActivateUI()
//     // {
//     //     if (repairPanelRoot != null) repairPanelRoot.SetActive(true);
//     // }

//     // public void DeactivateUI()
//     // {
//     //     if (repairPanelRoot != null) repairPanelRoot.SetActive(false);
//     // }

//     //ratchet noise, fired on an interval by PipeRepairSystem while a control is being moved.
//     public void PlayMoveSFX()
//     {
//         if (repairMoveSFX == null) return;
//         AudioEventBus.RequestSFX(new SFXEvent(repairMoveSFX, volume: 1f, pitch: Random.Range(0.9f, 1.1f), pos: transform.position));
//     }

//     public void PlayRepairDoneSFX()
//     {
//         if (repairDoneSFX == null) return;
//         AudioEventBus.RequestSFX(new SFXEvent(repairDoneSFX, volume: 1f, pitch: 1f, pos: transform.position));
//     }

//     //called every frame by PipeRepairSystem.Update() (same shape as GlobalModulationUI.UpdateVisuals)
//     public void UpdateVisuals(float[] currentValues, bool[] burst, int[] modes)
//     {
//         for (int i = 0; i < 4; i++)
//         {
//             //move the marker to match the live control value
//             ApplyMarker(channelMarkers[i], currentValues[i], modes[i]);

//             //leak overlay is shown while the pipe/valve is burst and hides the moment it is repaired
//             if (leakOverlays[i] != null)
//             {
//                 leakOverlays[i].SetActive(burst[i]);
//             }
//         }
//     }

//     //sets the rotary and slider markers to match. Ported from GlobalModulationUI.ApplyMarker,
//     //with the horizontal-slider assignment fixed so both slider modes actually move the marker.
//     private void ApplyMarker(RectTransform rt, float value, int mode)
//     {
//         if (rt == null) return;

//         if (mode == 0) //rotary
//         {
//             rt.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(rotaryMinDeg, rotaryMaxDeg, value));
//         }
//         else //sliders (1 = horizontal, 2 = vertical)
//         {
//             Vector2 pos = rt.anchoredPosition;
//             if (mode == 1)
//                 pos.x = Mathf.Lerp(-sliderHalfWidth, sliderHalfWidth, value);
//             else
//                 pos.y = Mathf.Lerp(-sliderHalfWidth, sliderHalfWidth, value);
//             rt.anchoredPosition = pos;
//         }
//     }
// }
