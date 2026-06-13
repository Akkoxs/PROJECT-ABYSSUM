using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Manages the blinking coroutines for the 4 warning light groups.
public class WarningLightController : MonoBehaviour
{
    // By creating a serializable class, we can group an expandable list of lights 
    // into a single channel array in the Unity Inspector.
    [System.Serializable]
    public class WarningChannel
    {
        [Tooltip("Add as many redundant lights as you want for this specific channel.")]
        public List<GameObject> lights = new List<GameObject>();
    }

    [Header("Warning Channels")]
    [Tooltip("The 4 channels. Expand each one to add primary and secondary lights.")]
    public WarningChannel[] channels = new WarningChannel[4];

    [Header("Settings")]
    [Tooltip("How fast the lights blink (in seconds).")]
    public float blinkInterval = 0.5f;

    [Header("Audio")]
    [Tooltip("Sound effect played each time the warning light pulses ON.")]
    [SerializeField] private AudioClip warningBlinkSFX;

    // Track the active coroutines and states
    private Coroutine[] flashCoroutines = new Coroutine[4];
    private bool[] isFlashing = new bool[4];

    public void SetLightState(int index, bool isLeaking)
    {
        // Safety check to avoid index out of bounds
        if (index < 0 || index >= channels.Length) return;

        if (isLeaking && !isFlashing[index])
        {
            // The pipe burst: mark as flashing and start the coroutine
            isFlashing[index] = true;
            flashCoroutines[index] = StartCoroutine(FlashLightRoutine(index));
        }
        else if (!isLeaking && isFlashing[index])
        {
            // The pipe was fixed: mark as false, stop the coroutine
            isFlashing[index] = false;
            if (flashCoroutines[index] != null)
            {
                StopCoroutine(flashCoroutines[index]);
            }
            
            // Force ALL redundant lights in this channel off
            foreach (GameObject lightObj in channels[index].lights)
            {
                if (lightObj != null) 
                {
                    lightObj.SetActive(false);
                }
            }
        }
    }

    private IEnumerator FlashLightRoutine(int index)
    {
        // Grab the list of all lights assigned to this specific channel
        List<GameObject> currentLights = channels[index].lights;

        // Loop continuously as long as this specific pipe is leaking
        while (isFlashing[index])
        {
            bool isLightOn = false;

            // Toggle every light in the list
            foreach (GameObject lightObj in currentLights)
            {
                if (lightObj != null)
                {
                    lightObj.SetActive(!lightObj.activeSelf); 
                    
                    // If the light just turned on, flag it so we can play the sound
                    if (lightObj.activeSelf) 
                    {
                        isLightOn = true;
                    }
                }
            }
            
            // Play the alarm beep only when the light pulses ON
            if (isLightOn && warningBlinkSFX != null)
            {
                AudioEventBus.RequestSFX(new SFXEvent(warningBlinkSFX, volume: 1f, pitch: 0.7f, pos: transform.position));
            }
            
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    public bool IsAnyChannelFlashing()
    {
        if (isFlashing == null) return false;
        
        for (int i = 0; i < isFlashing.Length; i++)
        {
            if (isFlashing[i]) return true; // Found an active fault
        }
        return false; // Everything is clean
    }
}