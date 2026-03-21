using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class MinigameManager : MonoBehaviour
{
    [Header("Line Renderers")]
    [SerializeField] private LineRenderer targetWaveLine; // Dark blue wave (target)
    [SerializeField] private LineRenderer playerWaveLine; // Light blue wave (player controlled)

    [Header("Wave Settings")]
    [SerializeField] private int resolution = 100; // Number of points in the wave
    [SerializeField] private float waveLength = 10f; // Length of the wave display
    [SerializeField] private float waveHeight = 0.3f; // Visual height scale

    [Header("Player Controls")]
    [SerializeField] private float amplitudeChangeSpeed = 1f; // Speed of amplitude change
    [SerializeField] private float shiftChangeSpeed = 1f; // Speed of horizontal shift change
    [SerializeField] private float minAmplitude = 1f;
    [SerializeField] private float maxAmplitude = 7f;
    [SerializeField] private float minShift = 1f;
    [SerializeField] private float maxShift = 7f;

    [Header("Matching")]
    [SerializeField] private float matchTolerance = 0.3f; // How close values need to be
    [SerializeField] private float matchTimeRequired = 1f; // How long to hold match

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI amplitudeText;
    [SerializeField] private TextMeshProUGUI shiftText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Minigame Scene Name")]
    [SerializeField] private string minigameName;

    // Target wave parameters (randomized)
    private float targetAmplitude;
    private float targetHorizontalShift;

    // Player wave parameters (controlled by input)
    private float playerAmplitude;
    private float playerHorizontalShift;

    private Vector2 dpadInput;
    private float matchTimer = 0f;
    private bool isMatched = false;

    void Start()
    {
        targetAmplitude = Random.Range(minAmplitude, maxAmplitude);
        targetHorizontalShift = Random.Range(minShift, maxShift);

        playerAmplitude = (minAmplitude + maxAmplitude) / 2f;
        playerHorizontalShift = (minShift + maxShift) / 2f;

        SetupLineRenderer(targetWaveLine, new Color(0f, 0.2f, 0.5f, 1f));
        SetupLineRenderer(playerWaveLine, new Color(0.3f, 0.6f, 1f, 1f));

        DrawWave(targetWaveLine, targetAmplitude, targetHorizontalShift);
        DrawWave(playerWaveLine, playerAmplitude, playerHorizontalShift);

        UpdateUI();
    }

    void Update()
    {
        if (isMatched) return;

        HandleInput();
        DrawWave(playerWaveLine, playerAmplitude, playerHorizontalShift);
        UpdateUI();
        CheckMatching();
    }

    void SetupLineRenderer(LineRenderer line, Color color)
    {
        line.positionCount = resolution;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.sortingOrder = 10;
    }

    void HandleInput()
    {
        float deltaTime = Time.unscaledDeltaTime;

        playerAmplitude += dpadInput.y * amplitudeChangeSpeed * deltaTime;
        playerAmplitude = Mathf.Clamp(playerAmplitude, minAmplitude, maxAmplitude);

        playerHorizontalShift += dpadInput.x * shiftChangeSpeed * deltaTime;
        playerHorizontalShift = Mathf.Clamp(playerHorizontalShift, minShift, maxShift);
    }

    void DrawWave(LineRenderer line, float amplitude, float horizontalShift)
    {
        Vector3[] points = new Vector3[resolution];

        for (int i = 0; i < resolution; i++)
        {
            float x = (i / (float)resolution) * waveLength;
            float y = amplitude * Mathf.Sin(x + horizontalShift) * waveHeight;
            points[i] = new Vector3(x - waveLength / 2f, y, 0);
        }

        line.SetPositions(points);
    }

    void UpdateUI()
    {
        if (amplitudeText != null)
        {
            amplitudeText.text = $"Amplitude: {playerAmplitude:F1}";
        }

        if (shiftText != null)
        {
            shiftText.text = $"Shift: {playerHorizontalShift:F1}";
        }
    }

    void CheckMatching()
    {
        float ampDiff = Mathf.Abs(playerAmplitude - targetAmplitude);
        float shiftDiff = Mathf.Abs(playerHorizontalShift - targetHorizontalShift);
        bool wavesMatch = ampDiff <= matchTolerance && shiftDiff <= matchTolerance;

        if (wavesMatch)
        {
            matchTimer += Time.unscaledDeltaTime;

            if (feedbackText != null)
            {
                feedbackText.text = $"MATCHING! ({matchTimer:F1}s / {matchTimeRequired}s)";
                feedbackText.color = Color.green;
            }

            playerWaveLine.startColor = Color.green;
            playerWaveLine.endColor = Color.green;

            if (matchTimer >= matchTimeRequired)
            {
                isMatched = true;
                CompleteMinigame();
            }
        }
        else
        {
            matchTimer = 0f;

            if (feedbackText != null)
            {
                feedbackText.text = "Match the target wave";
                feedbackText.color = Color.white;
            }

            playerWaveLine.startColor = new Color(0.3f, 0.6f, 1f, 1f);
            playerWaveLine.endColor = new Color(0.3f, 0.6f, 1f, 1f);
        }
    }

    void CompleteMinigame()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "COMPLETE!";
            feedbackText.color = Color.yellow;
        }

        Debug.Log("Sine wave matched!");
        Invoke(nameof(CloseMinigame), 1f);
    }

    void CloseMinigame()
    {
        enabled = false;
        Time.timeScale = 1f;
        SceneManager.UnloadSceneAsync(minigameName);
    }

    public void OnResize(InputAction.CallbackContext context)
    {
        dpadInput = context.ReadValue<Vector2>();
    }
}