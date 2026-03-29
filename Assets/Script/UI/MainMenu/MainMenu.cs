using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject creditsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] Button playButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button quitButton;

    [Header("Player Name")]
    [SerializeField] TMP_InputField playerNameInput;

    [Header("Settings")]
    [SerializeField] Slider volumeSlider;
    [SerializeField] TMP_Text volumeText;
    [SerializeField] TMP_Dropdown qualityDropdown;

    [Header("Buttons")]
    [SerializeField] Button backFromSettingsButton;
    [SerializeField] Button backFromCreditsButton;

    private void Start()
    {
        // Setup button listeners
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCreditsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(OnBackClicked);

        if (backFromCreditsButton != null)
            backFromCreditsButton.onClick.AddListener(OnBackClicked);

        // Load saved player name
        if (playerNameInput != null)
        {
            string savedName = PlayerPrefs.GetString("PlayerNickName", "Player");
            playerNameInput.text = savedName;
            playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
        }

        // Load settings
        LoadSettings();

        // Show main panel
        ShowMainPanel();
    }

    private void LoadSettings()
    {
        // Load volume
        if (volumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.value = volume;
            volumeText.text = $"{volume}%";
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // Load quality
        if (qualityDropdown != null)
        {
            int quality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            qualityDropdown.value = quality;
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }
    }

    #region Button Handlers

    public void OnPlayClicked()
    {
        SavePlayerName();
        SceneManager.LoadScene("Menu");
    }

    public void OnSettingsClicked()
    {
        ShowSettingsPanel();
    }

    public void OnCreditsClicked()
    {
        ShowCreditsPanel();
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnBackClicked()
    {
        ShowMainPanel();
    }

    public void OnPlayerNameChanged(string name)
    {
        SavePlayerName();
    }

    public void OnVolumeChanged(float volume)
    {
        AudioListener.volume = volume;
        volumeText.text = $"{volume}%";
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void OnQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        PlayerPrefs.Save();
    }

    #endregion

    #region Panel Management

    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ShowSettingsPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ShowCreditsPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    #endregion

    #region Save/Load

    private void SavePlayerName()
    {
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
        {
            PlayerPrefs.SetString("PlayerNickName", playerNameInput.text);
            PlayerPrefs.Save();
        }
    }

    #endregion
}