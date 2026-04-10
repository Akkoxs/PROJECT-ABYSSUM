using UnityEngine;
using TMPro;

public class MinigameManager : MonoBehaviour
{

    //DEPRECATED
    //I believe this script is not actually used in the game.

    [Header("Waves")]
    [SerializeField] private LineRenderer targetWaveLine;
    [SerializeField] private LineRenderer playerWaveLine;
    [SerializeField] private int wavePoints = 80;

    [Header("Target Wave Ranges")]
    [SerializeField] private float amplitudeMin = 0.5f;
    [SerializeField] private float amplitudeMax = 2.5f;
    [SerializeField] private float hShiftMin = 0.5f;
    [SerializeField] private float hShiftMax = 3.0f;
    [SerializeField] private float axisMin = -1.0f;
    [SerializeField] private float axisMax = 1.0f;
    [SerializeField] private float phaseMin = 0.0f;
    [SerializeField] private float phaseMax = Mathf.PI * 2f;

    [Header("Matching")]
    [SerializeField] private float matchThreshold = 0.12f;
    [SerializeField] private float matchHoldDuration = 1.5f;

    [Header("UI")]
    [SerializeField] private Canvas minigameCanvas;
    [SerializeField] private TextMeshProUGUI amplitudeText;
    [SerializeField] private TextMeshProUGUI hShiftText;
    [SerializeField] private TextMeshProUGUI axisText;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    private Camera gameCamera;
    private MinigameTrigger triggerController;

    private float targetAmplitude, targetHShift, targetAxis, targetPhase;
    private float playerAmplitude, playerHShift, playerAxis, playerPhase;

    private float matchTimer;
    private bool isComplete;
    private bool initialized;

    // ── Initialization ───────────────────────────────────────────────────────

    public void Initialize(Camera gameCam, Camera uiCam, MinigameTrigger controller)
    {
        gameCamera = gameCam;
        triggerController = controller;
        matchTimer = 0f;
        isComplete = false;

        RandomizeTarget();
        SetupLineRenderers();
        SetupCanvas(uiCam);

        initialized = true;
        Debug.Log("MinigameManager initialized successfully");
    }

    private void RandomizeTarget()
    {
        targetAmplitude = Random.Range(amplitudeMin, amplitudeMax);
        targetHShift = Random.Range(hShiftMin, hShiftMax);
        targetAxis = Random.Range(axisMin, axisMax);
        targetPhase = Random.Range(phaseMin, phaseMax);
    }

    private void SetupLineRenderers()
    {
        int layer = LayerMask.NameToLayer("Minigame");

        SetupLR(targetWaveLine, new Color(0f, 0.3f, 0.8f), 100, layer);
        SetupLR(playerWaveLine, new Color(0.3f, 0.6f, 1f), 101, layer);
    }

    private void SetupLR(LineRenderer lr, Color color, int sortOrder, int layer)
    {
        if (lr == null) return;
        lr.useWorldSpace = true;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.sortingLayerName = "Default";
        lr.sortingOrder = sortOrder;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        if (layer != -1) lr.gameObject.layer = layer;
    }

    private void SetupCanvas(Camera uiCam)
    {
        if (minigameCanvas == null) return;

        if (uiCam != null)
        {
            minigameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            minigameCanvas.worldCamera = uiCam;
            minigameCanvas.sortingOrder = 50;
            minigameCanvas.planeDistance = 0.05f;
        }
        else
        {
            minigameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        SetLayerRecursively(minigameCanvas.gameObject, LayerMask.NameToLayer("UI"));
    }

    // ── Runtime ──────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!initialized || isComplete || gameCamera == null) return;

        ReadSerialInput();
        DrawWave(targetWaveLine, targetAmplitude, targetHShift, targetAxis, targetPhase);
        DrawWave(playerWaveLine, playerAmplitude, playerHShift, playerAxis, playerPhase);
        CheckMatch();
        UpdateUI();
    }

    private void ReadSerialInput()
    {
        if (SerialHandler.Instance == null) return;

        playerAmplitude = Mathf.Lerp(amplitudeMin, amplitudeMax, SerialHandler.Instance.playerPot_a);
        playerHShift = Mathf.Lerp(hShiftMin, hShiftMax, SerialHandler.Instance.playerSlider_h);
        playerAxis = Mathf.Lerp(axisMin, axisMax, SerialHandler.Instance.playerPot_k);
        playerPhase = Mathf.Lerp(phaseMin, phaseMax, SerialHandler.Instance.playerSlider_c);
    }

    private void DrawWave(LineRenderer lr, float a, float h, float k, float c)
    {
        if (lr == null || gameCamera == null) return;

        float orthoH = gameCamera.orthographicSize;
        float orthoW = orthoH * gameCamera.aspect;

        // Camera world position — all wave points are offset from here
        Vector3 camPos = gameCamera.transform.position;
        Debug.Log($"Camera: {gameCamera.name} | camPos: {camPos} | orthoH: {orthoH} | orthoW: {orthoW} | isOrtho: {gameCamera.orthographic}");

        // Z just in front of the camera's near clip plane
        float wz = camPos.z + gameCamera.nearClipPlane + 0.1f;

        // Scale amplitude to a fraction of the camera's visible height
        float ampScale = orthoH * 0.25f / amplitudeMax;
        float axisScale = orthoH * 0.25f / Mathf.Max(Mathf.Abs(axisMin), Mathf.Abs(axisMax));

        lr.positionCount = wavePoints;

        for (int i = 0; i < wavePoints; i++)
        {
            float t = (float)i / (wavePoints - 1);

            // X spans 80% of visible width, centered on camera
            float x = camPos.x + Mathf.Lerp(-orthoW * 0.8f, orthoW * 0.8f, t);

            // Y centered on camera, offset by axis (k) and amplitude (a)
            float y = camPos.y
                    + (a * ampScale) * Mathf.Sin(h * t * Mathf.PI * 2f + c)
                    + (k * axisScale * Mathf.Cos(h * t * Mathf.PI * 2f + c));

            lr.SetPosition(i, new Vector3(x, y, wz));
        }

        // Force bounds to cover the full camera view so Unity never culls it
        lr.bounds = new Bounds(
            new Vector3(camPos.x, camPos.y, wz),
            new Vector3(orthoW * 3f, orthoH * 3f, 1f)
        );
    }

    // ── Match checking ───────────────────────────────────────────────────────

    private void CheckMatch()
    {
        bool ampMatch = Mathf.Abs(playerAmplitude - targetAmplitude) < matchThreshold * (amplitudeMax - amplitudeMin);
        bool hMatch = Mathf.Abs(playerHShift - targetHShift) < matchThreshold * (hShiftMax - hShiftMin);
        bool axisMatch = Mathf.Abs(playerAxis - targetAxis) < matchThreshold * (axisMax - axisMin);
        bool phaseMatch = Mathf.Abs(playerPhase - targetPhase) < matchThreshold * (phaseMax - phaseMin);

        bool matching = ampMatch && hMatch && axisMatch && phaseMatch;

        if (matching)
        {
            matchTimer += Time.deltaTime;
            SetPlayerWaveColor(Color.green);
            if (matchTimer >= matchHoldDuration) CompleteMinigame();
        }
        else
        {
            matchTimer = 0f;
            SetPlayerWaveColor(new Color(0.3f, 0.6f, 1f));
        }
    }

    private void SetPlayerWaveColor(Color c)
    {
        if (!playerWaveLine) return;
        playerWaveLine.startColor = c;
        playerWaveLine.endColor = c;
    }

    private void UpdateUI()
    {
        if (amplitudeText) amplitudeText.text = $"A: {playerAmplitude:F2}  →  {targetAmplitude:F2}";
        if (hShiftText) hShiftText.text = $"H: {playerHShift:F2}  →  {targetHShift:F2}";
        if (axisText) axisText.text = $"K: {playerAxis:F2}  →  {targetAxis:F2}";
        if (phaseText) phaseText.text = $"C: {playerPhase:F2}  →  {targetPhase:F2}";

        if (feedbackText && !isComplete)
        {
            int closeCount = 0;
            if (Mathf.Abs(playerAmplitude - targetAmplitude) < matchThreshold * 2f * (amplitudeMax - amplitudeMin)) closeCount++;
            if (Mathf.Abs(playerHShift - targetHShift) < matchThreshold * 2f * (hShiftMax - hShiftMin)) closeCount++;
            if (Mathf.Abs(playerAxis - targetAxis) < matchThreshold * 2f * (axisMax - axisMin)) closeCount++;
            if (Mathf.Abs(playerPhase - targetPhase) < matchThreshold * 2f * (phaseMax - phaseMin)) closeCount++;

            feedbackText.text = closeCount switch
            {
                4 => "Almost there!",
                3 => "Getting close...",
                _ => "Match the wave!"
            };
        }
    }

    private void CompleteMinigame()
    {
        isComplete = true;
        if (feedbackText) { feedbackText.text = "COMPLETE!"; feedbackText.color = Color.yellow; }
        Invoke(nameof(Close), 1.5f);
    }

    private void Close()
    {
        initialized = false;
        if (triggerController != null) triggerController.CloseMinigame();
        else gameObject.SetActive(false);
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        if (layer == -1) return;
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}