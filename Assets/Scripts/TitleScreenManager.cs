using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Attach this to the Canvas GameObject in your TitleScreen scene.
// Manages Main and Credits panels with keyboard navigation and fade to gameplay.

public class TitleScreenManager : MonoBehaviour
{
    // ───────────────────────────────────────────────
    //  PANELS  — assign in Inspector
    // ───────────────────────────────────────────────
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject creditsPanel;

    // ───────────────────────────────────────────────
    //  MAIN MENU BUTTONS  — assign in Inspector
    // ───────────────────────────────────────────────
    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button creditsButton;

    // ───────────────────────────────────────────────
    //  BACK BUTTONS  — assign in Inspector
    // ───────────────────────────────────────────────
    [Header("Back Buttons")]
    public Button creditsBackButton;

    // ───────────────────────────────────────────────
    //  BUTTON HIGHLIGHT  — glowing selected state
    // ───────────────────────────────────────────────
    [Header("Button Highlight Colors")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.6f);
    public Color selectedColor = new Color(1f, 0.85f, 0f, 1f);  // gold glow

    // ───────────────────────────────────────────────
    //  FADE OVERLAY  — assign a full-screen black Image
    // ───────────────────────────────────────────────
    [Header("Fade")]
    public Image fadeOverlay;
    public float fadeDuration = 1.2f;

    // ───────────────────────────────────────────────
    //  MUSIC
    // ───────────────────────────────────────────────
    [Header("Music")]
    [SerializeField] private AudioClip titleMusic;

    // ───────────────────────────────────────────────
    //  PRIVATE STATE
    // ───────────────────────────────────────────────
    private Button[] mainButtons;
    private int selectedIndex = 0;
    private bool navigating = false;

    // ───────────────────────────────────────────────
    //  INIT
    // ───────────────────────────────────────────────
    void Start()
    {
        mainButtons = new Button[] { startButton, creditsButton };

        startButton.onClick.AddListener(OnStart);
        creditsButton.onClick.AddListener(OnCredits);
        creditsBackButton.onClick.AddListener(OnBack);

        ShowPanel(mainPanel);

        if (fadeOverlay != null)
            StartCoroutine(FadeIn());

        UpdateButtonHighlights();

        if (titleMusic != null)
            AudioEventBus.RequestMusic(new MusicEvent(titleMusic, fadeDuration: 1f, loop: true));
    }

    // ───────────────────────────────────────────────
    //  UPDATE — keyboard / WASD navigation
    // ───────────────────────────────────────────────
    void Update()
    {
        if (navigating) return;

        if (creditsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Backspace))
                OnBack();
            return;
        }

        if (!mainPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + mainButtons.Length) % mainButtons.Length;
            UpdateButtonHighlights();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % mainButtons.Length;
            UpdateButtonHighlights();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            mainButtons[selectedIndex].onClick.Invoke();
        }
    }

    // ───────────────────────────────────────────────
    //  BUTTON ACTIONS
    // ───────────────────────────────────────────────
    void OnStart()
    {
        if (navigating) return;
        AudioEventBus.StopMusic();
        StartCoroutine(FadeToScene("FinalMaster"));
    }

    void OnCredits()
    {
        ShowPanel(creditsPanel);
    }

    void OnBack()
    {
        ShowPanel(mainPanel);
        UpdateButtonHighlights();
    }

    // ───────────────────────────────────────────────
    //  PANEL SWITCHING
    // ───────────────────────────────────────────────
    void ShowPanel(GameObject panel)
    {
        mainPanel.SetActive(panel == mainPanel);
        creditsPanel.SetActive(panel == creditsPanel);
    }

    // ───────────────────────────────────────────────
    //  BUTTON HIGHLIGHT
    // ───────────────────────────────────────────────
    void UpdateButtonHighlights()
    {
        for (int i = 0; i < mainButtons.Length; i++)
        {
            var tmp = mainButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (tmp == null) continue;
            tmp.color = (i == selectedIndex) ? selectedColor : normalColor;
        }
    }

    // ───────────────────────────────────────────────
    //  FADE COROUTINES
    // ───────────────────────────────────────────────
    IEnumerator FadeIn()
    {
        SetFadeAlpha(1f);
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetFadeAlpha(1f - t / fadeDuration);
            yield return null;
        }
        SetFadeAlpha(0f);
    }

    IEnumerator FadeToScene(string sceneName)
    {
        navigating = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetFadeAlpha(t / fadeDuration);
            yield return null;
        }
        SetFadeAlpha(1f);
        SceneManager.LoadScene(sceneName);
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay == null) return;
        var c = fadeOverlay.color;
        c.a = alpha;
        fadeOverlay.color = c;
    }
}