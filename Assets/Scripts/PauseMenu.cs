using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Main Menu Buttons")]
    public Button optionsButton;
    public Button titleScreenButton;

    [Header("Options")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;
    public Button optionsBackButton;

    [Header("Button Highlight Colors")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.6f);
    public Color selectedColor = new Color(0f, 1f, 1f, 1f);

    [Header("Scene Names")]
    public string titleSceneName = "TitleScreen";

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
    private float volumeStep = 0.05f;
    private Transform[] originalTargets;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        menuButtons = new Button[] { optionsButton, titleScreenButton };

        optionsButton.onClick.AddListener(OpenOptions);
        titleScreenButton.onClick.AddListener(GoToTitleScreen);
        optionsBackButton.onClick.AddListener(CloseOptions);

        originalTargets = new Transform[camerasToRedirect.Length];
        for (int i = 0; i < camerasToRedirect.Length; i++)
            if (camerasToRedirect[i] != null)
                originalTargets[i] = camerasToRedirect[i].target;

        float saved = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = saved;
        if (volumeSlider != null)
        {
            volumeSlider.value = saved;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        UpdateVolumeDisplay(saved);

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
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

        if (!isPaused) return;

        if (inOptions)
            HandleOptionsInput();
        else
            HandleMainMenuInput();
    }

    void OpenPause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
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
        optionsPanel.SetActive(false);
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
        optionsPanel.SetActive(true);
        if (volumeSlider != null)
            volumeSlider.value = AudioListener.volume;
        UpdateVolumeDisplay(AudioListener.volume);
    }

    void CloseOptions()
    {
        inOptions = false;
        optionsPanel.SetActive(false);
        PlayerPrefs.SetFloat("MasterVolume", AudioListener.volume);
        PlayerPrefs.Save();
    }

    void HandleOptionsInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            ChangeVolume(volumeStep);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            ChangeVolume(-volumeStep);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Backspace))
            CloseOptions();
    }

    void ChangeVolume(float delta)
    {
        float newVol = Mathf.Clamp01(AudioListener.volume + delta);
        AudioListener.volume = newVol;
        if (volumeSlider != null)
            volumeSlider.value = newVol;
        UpdateVolumeDisplay(newVol);
    }

    void OnVolumeChanged(float val)
    {
        AudioListener.volume = val;
        UpdateVolumeDisplay(val);
    }

    void UpdateVolumeDisplay(float vol)
    {
        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(vol * 100f) + "%";
    }
}