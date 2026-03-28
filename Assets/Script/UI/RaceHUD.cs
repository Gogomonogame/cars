using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceHUD : MonoBehaviour
{
    [Header("Speedometer")]
    public TMP_Text speedText;
    public Image speedometerNeedle;
    public float maxSpeed = 100f;
    public float minNeedleAngle = 0f;
    public float maxNeedleAngle = 180f;

    [Header("Position")]
    public TMP_Text positionText;

    [Header("Lap Counter")]
    public TMP_Text lapText;
    public int totalLaps = 2;

    [Header("Timer")]
    public TMP_Text timerText;

    [Header("Countdown")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button quitButton;

    private CarController localCarController;
    private CarLapCounter localLapCounter;
    private NetworkPlayer localPlayer;
    private float raceStartTime;
    private bool isPaused = false;

    private void Start()
    {
        // Find local player's car
        StartCoroutine(FindLocalCar());

        // Setup pause menu
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToMenu);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        totalLaps = 2; // Default, can be read from RaceManager
    }

    private System.Collections.IEnumerator FindLocalCar()
    {
        // Wait for local player to be spawned
        while (NetworkPlayer.Local == null)
        {
            yield return null;
        }

        localPlayer = NetworkPlayer.Local;
        localCarController = localPlayer.GetComponent<CarController>();
        localLapCounter = localPlayer.GetComponent<CarLapCounter>();

        // Wait for race to start
        while (RaceManager.Instance == null || !RaceManager.Instance.RaceStarted)
        {
            yield return null;
        }

        raceStartTime = Time.time;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        UpdateHUD();
    }

    private void UpdateHUD()
    {
        // Update speedometer
        if (localCarController != null && speedText != null)
        {
            float speed = localCarController.GetVelocityMagnitude();
            speedText.text = Mathf.RoundToInt(speed * 3.6f) + " km/h"; // Convert to km/h

            if (speedometerNeedle != null)
            {
                float normalizedSpeed = Mathf.Clamp01(speed / maxSpeed);
                float angle = Mathf.Lerp(minNeedleAngle, maxNeedleAngle, normalizedSpeed);
                speedometerNeedle.transform.localRotation = Quaternion.Euler(0, 0, -angle);
            }
        }

        // Update position
        if (localPlayer != null && RaceManager.Instance != null && positionText != null)
        {
            int position = RaceManager.Instance.GetPlayerPosition(localPlayer);
            string suffix = "th";
            if (position == 1) suffix = "st";
            else if (position == 2) suffix = "nd";
            else if (position == 3) suffix = "rd";

            positionText.text = $"{position}{suffix}";
        }

        // Update lap counter
        if (localLapCounter != null && lapText != null)
        {
            int currentLap = Mathf.Min(localLapCounter.GetNumberOfCheckpointsPassed() + 1, totalLaps);
            lapText.text = $"Lap {currentLap}/{totalLaps}";
        }

        // Update timer
        if (RaceManager.Instance != null && RaceManager.Instance.RaceStarted && timerText != null)
        {
            float elapsed = Time.time - raceStartTime;
            timerText.text = FormatTime(elapsed);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 1000) % 1000);
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }

    #region Countdown

    public void ShowCountdown(int value)
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);

            if (countdownText != null)
            {
                if (value > 0)
                    countdownText.text = value.ToString();
                else
                    countdownText.text = "GO!";
            }
        }
    }

    public void HideCountdown()
    {
        if (countdownPanel != null)
            countdownPanel.SetActive(false);
    }

    #endregion

    #region Pause Menu

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;

        // Leave the room
        if (Fusion.NetworkRunner.GetRunnerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()) != null)
        {
            var runner = Fusion.NetworkRunner.GetRunnerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            runner.Shutdown();
        }

        // Load main menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    #endregion

    #region Results

    public void ShowResults(string[] playerNames, float[] times)
    {
        // This will be handled by RaceManager
    }

    #endregion
}