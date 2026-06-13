using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// Put this on a manager object in your DeathScene and WinScene.
// Provides gold-highlight keyboard/gamepad navigation (matching the title screen)
// plus mouse clicking.

public class EndScreenManager : MonoBehaviour
{
    [Header("Scene Names (must match Build Settings exactly)")]
    [SerializeField] private string gameplaySceneName = "FinalMaster";
    [SerializeField] private string mainMenuSceneName = "TitleScreen";

    [Header("Buttons (order = navigation order)")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Button Highlight Colors")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.6f);
    public Color selectedColor = new Color(1f, 0.85f, 0f, 1f);  // gold glow

    private Button[] buttons;
    private int selectedIndex = 0;

    private InputAction upAction;
    private InputAction downAction;
    private InputAction confirmAction;

    void Start()
    {
        Time.timeScale = 1f; // make sure time is running on this screen

        buttons = new Button[] { retryButton, mainMenuButton };

        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(MainMenu);

        UpdateButtonHighlights();

        // Gamepad navigation
        upAction = new InputAction(type: InputActionType.Button, binding: "<Gamepad>/dpad/up");
        upAction.Enable();
        downAction = new InputAction(type: InputActionType.Button, binding: "<Gamepad>/dpad/down");
        downAction.Enable();
        confirmAction = new InputAction(type: InputActionType.Button, binding: "<Gamepad>/buttonSouth");
        confirmAction.Enable();
    }

    void OnDisable()
    {
        upAction?.Disable();
        downAction?.Disable();
        confirmAction?.Disable();
    }

    // ── Input helpers (keyboard + gamepad) ──
    bool UpPressed()
    {
        var kb = Keyboard.current;
        bool key = kb != null && (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame);
        return key || (upAction != null && upAction.WasPressedThisFrame());
    }

    bool DownPressed()
    {
        var kb = Keyboard.current;
        bool key = kb != null && (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame);
        return key || (downAction != null && downAction.WasPressedThisFrame());
    }

    bool ConfirmPressed()
    {
        var kb = Keyboard.current;
        bool key = kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame);
        return key || (confirmAction != null && confirmAction.WasPressedThisFrame());
    }

    void Update()
    {
        if (buttons == null || buttons.Length == 0) return;

        if (UpPressed())
        {
            selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
            UpdateButtonHighlights();
        }
        else if (DownPressed())
        {
            selectedIndex = (selectedIndex + 1) % buttons.Length;
            UpdateButtonHighlights();
        }

        if (ConfirmPressed())
        {
            if (buttons[selectedIndex] != null)
                buttons[selectedIndex].onClick.Invoke();
        }
    }

    void UpdateButtonHighlights()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            var tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (tmp == null) continue;
            tmp.color = (i == selectedIndex) ? selectedColor : normalColor;
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}