using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;

    [Header("Main Menu Buttons")]
    public Button optionsButton;
    public Button titleScreenButton;

    [Header("Button Highlight Colors")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.6f);
    public Color selectedColor = new Color(0f, 1f, 1f, 1f);

    [Header("Scene Names")]
    public string titleSceneName = "TitleScreen";
    public string optionsSceneName = "OptionsScreen";

    [Header("Holocade Cameras")]
    public Cam2DFollow[] camerasToRedirect;
    public Transform pauseMenuTarget;

    [Header("Audio")]
    public AudioClip menuOpenSound;
    private AudioSource audioSource;

    private Button[] menuButtons;
    private int selectedIndex = 0;
    private bool isPaused = false;
    private bool inOptions = false;
    private Transform[] originalTargets;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        menuButtons = new Button[] { optionsButton, titleScreenButton };

        optionsButton.onClick.AddListener(OpenOptions);
        titleScreenButton.onClick.AddListener(GoToTitleScreen);

        originalTargets = new Transform[camerasToRedirect.Length];
        for (int i = 0; i < camerasToRedirect.Length; i++)
            if (camerasToRedirect[i] != null)
                originalTargets[i] = camerasToRedirect[i].target;

        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (inOptions)
                CloseOptions();
            else if (isPaused)
                ClosePause();
            else
                OpenPause();
            return;
        }

        if (!isPaused || inOptions) return;

        HandleMainMenuInput();
    }

    void OpenPause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        selectedIndex = 0;
        UpdateButtonHighlights();

        if (audioSource != null && menuOpenSound != null)
        {
            audioSource.clip = menuOpenSound;
            audioSource.Play();
        }

        foreach (var cam in camerasToRedirect)
            if (cam != null) cam.target = pauseMenuTarget;
    }

    void ClosePause()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        inOptions = false;

        if (audioSource != null) StartCoroutine(FadeOutAudio(0.5f));

        for (int i = 0; i < camerasToRedirect.Length; i++)
            if (camerasToRedirect[i] != null && originalTargets[i] != null)
                camerasToRedirect[i].target = originalTargets[i];
    }

    void HandleMainMenuInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + menuButtons.Length) % menuButtons.Length;
            UpdateButtonHighlights();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % menuButtons.Length;
            UpdateButtonHighlights();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            menuButtons[selectedIndex].onClick.Invoke();
        }
    }

    void UpdateButtonHighlights()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            var tmp = menuButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (tmp == null) continue;
            tmp.color = (i == selectedIndex) ? selectedColor : normalColor;
        }
    }

    IEnumerator FadeOutAudio(float duration)
    {
        float startVolume = audioSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    void GoToTitleScreen()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeOutAndLoad(titleSceneName));
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        if (audioSource != null)
        {
            float startVolume = audioSource.volume;
            float t = 0f;
            float duration = 0.5f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
                yield return null;
            }
            audioSource.Stop();
            audioSource.volume = startVolume;
        }
        SceneManager.LoadScene(sceneName);
    }

    void OpenOptions()
    {
        inOptions = true;
        StartCoroutine(LoadOptionsScene());
    }

    void CloseOptions()
    {
        inOptions = false;
        PlayerPrefs.SetFloat("MasterVolume", AudioListener.volume);
        PlayerPrefs.Save();
        StartCoroutine(UnloadOptionsScene());
    }

    IEnumerator LoadOptionsScene()
    {
        yield return SceneManager.LoadSceneAsync(optionsSceneName, LoadSceneMode.Additive);
    }

    IEnumerator UnloadOptionsScene()
    {
        yield return SceneManager.UnloadSceneAsync(optionsSceneName);
    }
}