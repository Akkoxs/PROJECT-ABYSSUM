using UnityEngine;
using TMPro;

public class MinigameManager : MonoBehaviour
{
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

    // Injected by MinigameTrigger
    private Camera gameCamera;
    private MinigameTrigger triggerController;

    // Target wave (randomized on init)
    private float targetAmplitude;
    private float targetHShift;
    private float targetAxis;
    private float targetPhase;

    // Player wave (driven by serial pots/sliders, remapped to same ranges)
    private float playerAmplitude;
    private float playerHShift;
    private float playerAxis;
    private float playerPhase;

    private float matchTimer;
    private bool isComplete;
    private bool initialized;

    // Wave draw bounds in world space
    private float waveXMin, waveXMax;
    private float targetWaveY, playerWaveY;

    // ── Initialization ───────────────────────────────────────────────────────

    public void Initialize(Camera gameCam, Camera uiCam, MinigameTrigger controller)
    {
        gameCamera = gameCam;
        triggerController = controller;
        matchTimer = 0f;
        isComplete = false;

        RandomizeTarget();
        SetMinigameLayer();
        SetupCanvas(uiCam);
        ComputeWaveBounds();

        initialized = true;
    }

    private void RandomizeTarget()
    {
        targetAmplitude = Random.Range(amplitudeMin, amplitudeMax);
        targetHShift = Random.Range(hShiftMin, hShiftMax);
        targetAxis = Random.Range(axisMin, axisMax);
        targetPhase = Random.Range(phaseMin, phaseMax);
    }

    private void SetMinigameLayer()
    {
        int layer = LayerMask.NameToLayer("Minigame");
        if (layer == -1) return;
        if (targetWaveLine) targetWaveLine.gameObject.layer = layer;
        if (playerWaveLine) playerWaveLine.gameObject.layer = layer;
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

    private void ComputeWaveBounds()
    {
        if (gameCamera == null) return;

        float orthoH = gameCamera.orthographicSize;
        float orthoW = orthoH * gameCamera.aspect;
        Vector3 c = gameCamera.transform.position;

        waveXMin = c.x - orthoW * 0.6f;
        waveXMax = c.x + orthoW * 0.6f;
        targetWaveY = c.y + orthoH * 0.3f;
        playerWaveY = c.y - orthoH * 0.3f;
    }

    // ── Runtime ──────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!initialized || isComplete || gameCamera == null) return;

        ReadSerialInput();
        DrawWave(targetWaveLine, targetAmplitude, targetHShift, targetAxis, targetPhase, targetWaveY);
        DrawWave(playerWaveLine, playerAmplitude, playerHShift, playerAxis, playerPhase, playerWaveY);
        CheckMatch();
        UpdateUI();
    }

    // Remap each 0-1 serial value to the same ranges used for the target
    private void ReadSerialInput()
    {
        if (SerialHandler.Instance == null) return;

        playerAmplitude = Mathf.Lerp(amplitudeMin, amplitudeMax, SerialHandler.Instance.playerPot_a);
        playerHShift = Mathf.Lerp(hShiftMin, hShiftMax, SerialHandler.Instance.playerSlider_h);
        playerAxis = Mathf.Lerp(axisMin, axisMax, SerialHandler.Instance.playerPot_k);
        playerPhase = Mathf.Lerp(phaseMin, phaseMax, SerialHandler.Instance.playerSlider_c);
    }

    // y = k + a * sin(h * t + c)
    // t is a normalized [0,1] position along the wave, mapped to one full period via 2π
    private void DrawWave(LineRenderer lr, float a, float h, float k, float c, float yCenter)
    {
        if (lr == null) return;
        lr.positionCount = wavePoints;

        for (int i = 0; i < wavePoints; i++)
        {
            float t = (float)i / (wavePoints - 1);                      // 0 → 1
            float x = Mathf.Lerp(waveXMin, waveXMax, t);
            float y = yCenter + k + a * Mathf.Sin(h * t * Mathf.PI * 2f + c);
            lr.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

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
            // Count how many params are close so feedback can be more specific
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

    // ── Helper ───────────────────────────────────────────────────────────────

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        if (layer == -1) return;
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}