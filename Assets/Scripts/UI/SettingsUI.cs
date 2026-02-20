using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Volume Sliders")]
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Volume Labels")]
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Hide settings panel at start
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Load saved volume settings
        LoadVolumeSettings();

        // Setup slider listeners
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void Update()
    {
        // Toggle settings panel with ESC key (only in preparation phase)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Only allow settings in preparation phase
            if (GameStateManager.Instance != null && GameStateManager.Instance.IsPreparationPhase)
            {
                ToggleSettings();
            }
        }
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);

            // Pause/unpause game when settings open/close
            Time.timeScale = !isActive ? 0f : 1f;
        }
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }

        if (bgmVolumeText != null)
        {
            bgmVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void LoadVolumeSettings()
    {
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = bgmVolume;
        if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(bgmVolume);
        if (bgmVolumeText != null) bgmVolumeText.text = $"{Mathf.RoundToInt(bgmVolume * 100)}%";

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(sfxVolume);
        if (sfxVolumeText != null) sfxVolumeText.text = $"{Mathf.RoundToInt(sfxVolume * 100)}%";
    }

    public void OnResetButtonClicked()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    public void OnQuitButtonClicked()
    {
        // 게임 상태 저장은 GameManager에 위임
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGameStateToDisk();
        }

        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnDestroy()
    {
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        Time.timeScale = 1f;
    }
}
