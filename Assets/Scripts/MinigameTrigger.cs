using UnityEngine;

public class MinigameTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject minigameRoot; // The parent GO holding all minigame objects
    [SerializeField] private Splitscreen splitscreen;

    // Which of the 4 cameras should see the minigame LineRenderers
    [SerializeField] private Camera minigameCamera;

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool oneTimeUse = true;

    private bool triggered = false;
    private int minigameMask;

    private void Start()
    {
        int layer = LayerMask.NameToLayer("Minigame");
        if (layer == -1)
        {
            Debug.LogError("MinigameTrigger: 'Minigame' layer missing. Add it in Project Settings > Tags and Layers.");
            return;
        }
        minigameMask = 1 << layer;

        // Strip Minigame layer from all 4 cameras so nothing leaks into other quadrants
        if (splitscreen != null)
            foreach (var cam in splitscreen.GetGameCameras())
                if (cam != null) cam.cullingMask &= ~minigameMask;

        // Make sure minigame is hidden at start
        if (minigameRoot != null) minigameRoot.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(playerTag)) return;
        if (triggered && oneTimeUse) return;

        OpenMinigame();
        if (oneTimeUse) triggered = true;
    }

    private void OpenMinigame()
    {
        // Allow the chosen camera to see the Minigame layer
        if (minigameCamera != null)
            minigameCamera.cullingMask |= minigameMask;

        if (minigameRoot != null)
        {
            minigameRoot.SetActive(true);
            // Pass the UI cam reference to the manager
            var mgr = minigameRoot.GetComponentInChildren<MinigameManager>();
            if (mgr != null)
            {
                Camera uiCam = splitscreen != null ? splitscreen.GetUICamera() : null;
                mgr.Initialize(minigameCamera, uiCam, this);
            }
        }
    }

    public void CloseMinigame()
    {
        // Revoke Minigame layer from the camera again
        if (minigameCamera != null)
            minigameCamera.cullingMask &= ~minigameMask;

        if (minigameRoot != null) minigameRoot.SetActive(false);
    }
}