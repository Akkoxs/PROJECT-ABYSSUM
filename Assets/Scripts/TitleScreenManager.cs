using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// Attach this to the Canvas GameObject in your TitleScreen scene.
// Manages Main, Credits, and Controls panels with keyboard + gamepad navigation and fade to gameplay.
// Controls panel cycles through separate image GameObjects (one active at a time).

public class TitleScreenManager : MonoBehaviour
{
    // ───────────────────────────────────────────────
    //  PANELS  — assign in Inspector
    // ───────────────────────────────────────────────
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject creditsPanel;
    public GameObject controlsPanel;

    // ───────────────────────────────────────────────
    //  MAIN MENU BUTTONS  — assign in Inspector
    // ───────────────────────────────────────────────
    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button creditsButton;
    public Button controlsButton;

    // ───────────────────────────────────────────────
    //  BACK BUTTONS  — assign in Inspector
    // ───────────────────────────────────────────────
    [Header("Back Buttons")]
    public Button creditsBackButton;
    public Button controlsBackButton;

    // ───────────────────────────────────────────────
    //  CONTROLS PANEL IMAGE CYCLING  — assign in Inspector
    //  Drag your four image GameObjects here, in order.
    // ───────────────────────────────────────────────
    [Header("Controls Panel")]
    public GameObject[] controlImageObjects;  // DiverImage, CaptainImage, SonarImage, CipherImage
    public Button controlsNextButton;
    public Button controlsPrevButton;
    public bool controlsWrapAround = true;
    private int controlsIndex = 0;

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

    private InputAction dpadUpAction;
    private InputAction dpadDownAction;
    private InputAction confirmAction;


    // ───────────────────────────────────────────────
    //  INIT
    // ───────────────────────────────────────────────
    void Start()
    {
        mainButtons = new Button[] { startButton, creditsButton, controlsButton };

        startButton.onClick.AddListener(OnStart);
        creditsButton.onClick.AddListener(OnCredits);
        controlsButton.onClick.AddListener(OnControls);
        creditsBackButton.onClick.AddListener(OnBack);
        controlsBackButton.onClick.AddListener(OnBack);

        if (controlsNextButton != null) controlsNextButton.onClick.AddListener(NextControl);
        if (controlsPrevButton != null) controlsPrevButton.onClick.AddListener(PrevControl);

        ShowPanel(mainPanel);

        if (fadeOverlay != null)
            StartCoroutine(FadeIn());

        UpdateButtonHighlights();

        if (titleMusic != null)
            AudioEventBus.RequestMusic(new MusicEvent(titleMusic, fadeDuration: 1f, loop: true));

        dpadUpAction = new InputAction(type: InputActionType.Button, binding: "<Gamepad>/dpad/up");
        dpadUpAction.Enable();

        dpadDownAction = new InputAction(type: InputActionType.Button, binding: "<Gamepad>/dpad/down");
        dpadDownAction.Enable();

        confirmAction = new InputAction(type: InputActionType.Button, binding: "<Gamepad>/buttonSouth");
        confirmAction.Enable();
    }

    void OnDisable()
    {
        dpadUpAction?.Disable();
        dpadDownAction?.Disable();
        confirmAction?.Disable();
    }

    // ───────────────────────────────────────────────
    //  KEYBOARD + GAMEPAD HELPERS
    // ───────────────────────────────────────────────
    bool UpPressed()
    {
        var kb = Keyboard.current;
        bool key = kb != null && (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame);
        return key || (dpadUpAction != null && dpadUpAction.WasPressedThisFrame());
    }

    bool DownPressed()
    {
        var kb = Keyboard.current;
        bool key = kb != null && (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame);
        return key || (dpadDownAction != null && dpadDownAction.WasPressedThisFrame());
    }

    bool LeftPressed()
    {
        var kb = Keyboard.current;
        bool key = kb != null && (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame);
        var gp = Gamepad.current;
        bool pad = gp != null && gp.dpad.left.wasPressedThisFrame;
        return key || pad;
    }

    bool RightPressed()
    {
        var kb = Keyboard.current;
        bool key = kb != null && (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame);
        var gp = Gamepad.current;
        bool pad = gp != null && gp.dpad.right.wasPressedThisFrame;
        return key || pad;
    }

    bool ConfirmPressed()
    {
        var kb = Keyboard.current;
        bool key = kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame);
        return key || (confirmAction != null && confirmAction.WasPressedThisFrame());
    }

    // ───────────────────────────────────────────────
    //  UPDATE
    // ───────────────────────────────────────────────
    void Update()
    {
        if (navigating) return;

        if (creditsPanel.activeSelf)
        {
            if (ConfirmPressed())
                OnBack();
            return;
        }

        if (controlsPanel != null && controlsPanel.activeSelf)
        {
            if (RightPressed()) NextControl();
            if (LeftPressed()) PrevControl();
            if (ConfirmPressed()) OnBack();
            return;
        }

        if (!mainPanel.activeSelf) return;

        if (UpPressed())
        {
            selectedIndex = (selectedIndex - 1 + mainButtons.Length) % mainButtons.Length;
            UpdateButtonHighlights();
        }
        else if (DownPressed())
        {
            selectedIndex = (selectedIndex + 1) % mainButtons.Length;
            UpdateButtonHighlights();
        }

        if (ConfirmPressed())
            mainButtons[selectedIndex].onClick.Invoke();
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

    void OnControls()
    {
        controlsIndex = 0;
        ShowCurrentControl();
        ShowPanel(controlsPanel);
    }

    void OnBack()
    {
        ShowPanel(mainPanel);
        UpdateButtonHighlights();
    }

    // ───────────────────────────────────────────────
    //  CONTROLS IMAGE CYCLING  (active GameObject swap)
    // ───────────────────────────────────────────────
    void NextControl()
    {
        if (controlImageObjects == null || controlImageObjects.Length == 0) return;
        controlsIndex++;
        if (controlsIndex >= controlImageObjects.Length)
            controlsIndex = controlsWrapAround ? 0 : controlImageObjects.Length - 1;
        ShowCurrentControl();
    }

    void PrevControl()
    {
        if (controlImageObjects == null || controlImageObjects.Length == 0) return;
        controlsIndex--;
        if (controlsIndex < 0)
            controlsIndex = controlsWrapAround ? controlImageObjects.Length - 1 : 0;
        ShowCurrentControl();
    }

    void ShowCurrentControl()
    {
        if (controlImageObjects == null) return;
        // Turn on only the current one, turn off the rest.
        for (int i = 0; i < controlImageObjects.Length; i++)
        {
            if (controlImageObjects[i] != null)
                controlImageObjects[i].SetActive(i == controlsIndex);
        }
    }

    // ───────────────────────────────────────────────
    //  PANEL SWITCHING
    // ───────────────────────────────────────────────
    void ShowPanel(GameObject panel)
    {
        mainPanel.SetActive(panel == mainPanel);
        creditsPanel.SetActive(panel == creditsPanel);
        if (controlsPanel != null)
            controlsPanel.SetActive(panel == controlsPanel);
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