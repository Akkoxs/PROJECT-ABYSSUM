using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseCanvas;

    [Header("Player 1 - New Input System")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string pauseActionName = "Pause";  // action name in your Input Actions asset
    [SerializeField] private string restartActionName = "Restart";

    private InputAction pauseAction;
    private InputAction restartAction;

    private bool isPaused = false;

    // Serial edge detection — done manually since SerialHandler.Update stops at timeScale=0
    private bool joy2WasPressed = false;
    private bool shootWasPressed = false;

    void Start()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (inputActions != null)
        {
            pauseAction = inputActions.FindAction(pauseActionName, throwIfNotFound: false);
            restartAction = inputActions.FindAction(restartActionName, throwIfNotFound: false);
            pauseAction?.Enable();
            restartAction?.Enable();
        }
    }

    void OnDisable()
    {
        pauseAction?.Disable();
        restartAction?.Disable();
    }

    void Update()
    {
        HandleGamepadPause();
        HandleSerialPause();
        HandleRestart();
    }

    void HandleGamepadPause()
    {
        if (pauseAction == null) return;

        if (pauseAction.WasPressedThisFrame())
            TogglePause();
    }

    void HandleSerialPause()
    {
        if (SerialHandler.Instance == null || !SerialHandler.Instance.IsSerialReady) return;


        bool joy2Pressed = SerialHandler.Instance.joy2X > 0.9f;

        if (joy2Pressed && !joy2WasPressed)
            TogglePause();

        joy2WasPressed = joy2Pressed;
    }

    void HandleRestart()
    {
        if (!isPaused)
        {
            shootWasPressed = false;
            return;
        }

        // Gamepad restart
        if (restartAction != null && restartAction.WasPressedThisFrame())
        {
            RestartScene();
            return;
        }

        // Serial restart — submarine shoot button while paused
        if (SerialHandler.Instance != null && SerialHandler.Instance.IsSerialReady)
        {
            bool shootPressed = SerialHandler.Instance.shoot;
            if (shootPressed && !shootWasPressed)
                RestartScene();
            shootWasPressed = shootPressed;
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pauseCanvas != null)
            pauseCanvas.SetActive(isPaused);
    }

    void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}