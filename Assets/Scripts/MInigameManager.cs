using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    //thanks claude for organizing
    [Header("Squares")]
    [SerializeField] private RectTransform target;
    [SerializeField] private RectTransform player;

    [Header("Target Size")]
    [SerializeField] private Vector2 targetSize = new Vector2(300f, 300f);

    [Header("Player Square Settings")]
    [SerializeField] private Vector2 startSize = new Vector2(100f, 100f);
    [SerializeField] private float resizeSpeed = 50f;
    [SerializeField] private float minSize = 50f;
    [SerializeField] private float maxSize = 500f;

    [Header("Matching")]
    [SerializeField] private float matchTolerance = 10f;
    [SerializeField] private float matchTimeRequired = 0.5f;

    [Header("UI Feedback")]
    [SerializeField] private Image playerImage;
    [SerializeField] private Color normalColor = new Color(0.3f, 0.6f, 1f, 1f);
    [SerializeField] private Color matchedColor = Color.green;

    private Vector2 currentPlayerSize;
    private Vector2 dpadInput;
    private float matchTimer = 0f;
    private bool isMatched = false;

    void Start()
    {
        if (target != null)
        {
            target.sizeDelta = targetSize;
        }

        currentPlayerSize = startSize;
        if (player != null)
        {
            player.sizeDelta = currentPlayerSize;
        }

        if (playerImage != null)
        {
            playerImage.color = normalColor;
        }
    }

    void Update()
    {
        HandleResizing();
        CheckMatching();
    }

    void HandleResizing()
    {
        if (isMatched) return;

        float resizeAmount = resizeSpeed * Time.unscaledDeltaTime;
        currentPlayerSize.y += dpadInput.y * resizeAmount;
        currentPlayerSize.x += dpadInput.x * resizeAmount;

        currentPlayerSize.x = Mathf.Clamp(currentPlayerSize.x, minSize, maxSize);
        currentPlayerSize.y = Mathf.Clamp(currentPlayerSize.y, minSize, maxSize);

        if (player != null)
        {
            player.sizeDelta = currentPlayerSize;
        }
    }

    void CheckMatching()
    {
        float widthDiff = Mathf.Abs(currentPlayerSize.x - targetSize.x);
        float heightDiff = Mathf.Abs(currentPlayerSize.y - targetSize.y);

        bool sizesMatch = widthDiff <= matchTolerance && heightDiff <= matchTolerance;

        if (sizesMatch && !isMatched)
        {
            matchTimer += Time.unscaledDeltaTime;

            if (playerImage != null)
            {
                playerImage.color = matchedColor;
            }

            if (matchTimer >= matchTimeRequired)
            {
                isMatched = true;
                CompleteMinigame();
            }
        }
        else
        {
            matchTimer = 0f;
            if (playerImage != null && !isMatched)
            {
                playerImage.color = normalColor;
            }
        }
    }

    void CompleteMinigame()
    {
        Debug.Log("Minigame completed!");
        Invoke(nameof(CloseMinigame), 0.5f);
    }

    void CloseMinigame()
    {
        Time.timeScale = 1f;
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }

    public void OnResize(InputAction.CallbackContext context)
    {
        dpadInput = context.ReadValue<Vector2>();
    }
}