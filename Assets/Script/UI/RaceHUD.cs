using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

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

    [Header("Minimap")]
    public RectTransform minimapIndicator;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button quitButton;

    private CarController localCarController;
    // Якщо твій скрипт називається ArcadeCarController, заміни тип тут
    private CarLapCounter localLapCounter;
    private NetworkPlayer localPlayer;
    private float raceStartTime;
    private bool isPaused = false;
    private bool raceTimerStarted = false;

    private void Start()
    {
        StartCoroutine(FindLocalCar());

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToMenu);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private System.Collections.IEnumerator FindLocalCar()
    {
        // Чекаємо локального гравця
        while (NetworkPlayer.Local == null)
        {
            yield return null;
        }

        localPlayer = NetworkPlayer.Local;
        localCarController = localPlayer.GetComponent<CarController>();
        localLapCounter = localPlayer.GetComponent<CarLapCounter>();

        // ЧЕКАЄМО, поки RaceManager з'явиться в мережі (Spawned)
        while (RaceManager.Instance == null || !RaceManager.Instance.Object || !RaceManager.Instance.Object.IsValid)
        {
            yield return null;
        }

        // Чекаємо саме початку гонки
        while (!RaceManager.Instance.RaceStarted)
        {
            yield return null;
        }

        raceStartTime = Time.time;
        raceTimerStarted = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // БЕЗПЕЧНА ПЕРЕВІРКА: чи існує RaceManager і чи він в мережі
        if (RaceManager.Instance == null || !RaceManager.Instance.Object || !RaceManager.Instance.Object.IsValid)
        {
            return;
        }

        UpdateHUD();
    }

    private void UpdateHUD()
    {
        // Спідометр
        if (localCarController != null && speedText != null)
        {
            // Переконайся, що в CarController є метод GetVelocityMagnitude()
            float speed = localCarController.GetVelocityMagnitude();
            speedText.text = Mathf.RoundToInt(speed * 3.6f) + " km/h";

            if (speedometerNeedle != null)
            {
                float normalizedSpeed = Mathf.Clamp01(speed / maxSpeed);
                float angle = Mathf.Lerp(minNeedleAngle, maxNeedleAngle, normalizedSpeed);
                speedometerNeedle.transform.localRotation = Quaternion.Euler(0, 0, -angle);
            }
        }

        // Позиція в гонці
        if (localPlayer != null && positionText != null)
        {
            int position = RaceManager.Instance.GetPlayerPosition(localPlayer);
            string suffix = GetOrdinalSuffix(position);
            positionText.text = $"{position}{suffix}";
        }

        // Лічильник кіл
        if (localLapCounter != null && lapText != null)
        {
            // Отримуємо кількість кіл з RaceManager (там вона мережева)
            int maxLaps = RaceManager.Instance.LapsToComplete;
            int currentLap = Mathf.Min(localLapCounter.GetCurrentLap() + 1, maxLaps);
            lapText.text = $"Lap {currentLap}/{maxLaps}";
        }

        // Таймер гонки
        if (raceTimerStarted && !RaceManager.Instance.RaceFinished && timerText != null)
        {
            float elapsed = Time.time - raceStartTime;
            timerText.text = FormatTime(elapsed);
        }
    }

    private string GetOrdinalSuffix(int num)
    {
        if (num <= 0) return "";
        switch (num)
        {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        }
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 1000) % 1000);
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(isPaused);

        // Увага: Time.timeScale впливає на фізику локально, але не зупиняє мережу!
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        // Вимикаємо Fusion Runner перед виходом
        var runners = FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None);
        foreach (var r in runners)
        {
            r.Shutdown();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}