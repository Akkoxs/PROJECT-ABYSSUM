using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseCanvas;

    [Header("Player 1 - Gamepad Bindings")]
    // START button on Xbox controller = joystick button 7
    [SerializeField] private string p1PauseButtonName = "joystick button 7";
    // A button on Xbox controller = joystick button 0
    [SerializeField] private string p1RestartButtonName = "joystick button 0";

    private bool isPaused = false;

    private bool p1PauseWasPressed = false;
    private bool p1RestartWasPressed = false;
    private bool p2RestartWasPressed = false;

    void Start()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (SerialHandler.Instance != null)
            SerialHandler.Instance.OnJoy2Clicked += TogglePause;
    }

    void OnDisable()
    {
        if (SerialHandler.Instance != null)
            SerialHandler.Instance.OnJoy2Clicked -= TogglePause;
    }

    void Update()
    {
        HandlePlayer1Input();
        HandlePlayer2SerialRestart();

        // Debug: log any joystick button press to identify correct button indices
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown("joystick button " + i))
                Debug.Log("Pressed: joystick button " + i);
        }
    }

    void HandlePlayer1Input()
    {
        // Manual edge detection so this works even at timeScale = 0
        bool pauseDown = Input.GetKey(p1PauseButtonName);
        if (pauseDown && !p1PauseWasPressed)
            TogglePause();
        p1PauseWasPressed = pauseDown;

        // Restart is only checked while paused, and uses a DIFFERENT button
        if (isPaused)
        {
            bool restartDown = Input.GetKey(p1RestartButtonName);
            if (restartDown && !p1RestartWasPressed)
                RestartScene();
            p1RestartWasPressed = restartDown;
        }
        else
        {
            p1RestartWasPressed = false;
        }
    }

    void HandlePlayer2SerialRestart()
    {
        if (!isPaused || SerialHandler.Instance == null || !SerialHandler.Instance.IsSerialReady)
        {
            p2RestartWasPressed = false;
            return;
        }

        // Assumes SerialHandler.shoot is a held boolean (true while shoot button is down).
        // If it's a one-frame pulse, this edge detection will miss it — see note below.
        bool shootDown = SerialHandler.Instance.shoot;
        if (shootDown && !p2RestartWasPressed)
            RestartScene();
        p2RestartWasPressed = shootDown;
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