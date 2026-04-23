using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class OptionsScreenManager : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    void Start()
    {
        // Load saved values
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        masterVolumeSlider.value = master;
        musicVolumeSlider.value = music;
        sfxVolumeSlider.value = sfx;

        UpdateDisplay(masterVolumeText, master);
        UpdateDisplay(musicVolumeText, music);
        UpdateDisplay(sfxVolumeText, sfx);

        masterVolumeSlider.onValueChanged.AddListener(OnMasterChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXChanged);

        backButton.onClick.AddListener(Back);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Backspace))
            Back();
    }

    void OnMasterChanged(float val)
    {
        AudioListener.volume = val;
        UpdateDisplay(masterVolumeText, val);
        PlayerPrefs.SetFloat("MasterVolume", val);
    }

    void OnMusicChanged(float val)
    {
        // Connect to your AudioMasterManager music volume
        PlayerPrefs.SetFloat("MusicVolume", val);
        UpdateDisplay(musicVolumeText, val);
    }

    void OnSFXChanged(float val)
    {
        // Connect to your AudioMasterManager SFX volume
        PlayerPrefs.SetFloat("SFXVolume", val);
        UpdateDisplay(sfxVolumeText, val);
    }

    void UpdateDisplay(TextMeshProUGUI label, float val)
    {
        if (label != null)
            label.text = Mathf.RoundToInt(val * 100f) + "%";
    }

    void Back()
    {
        PlayerPrefs.Save();
        SceneManager.UnloadSceneAsync(gameObject.scene.name);
    }
}