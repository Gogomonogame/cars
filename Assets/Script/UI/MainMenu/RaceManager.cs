using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class RaceManager : NetworkBehaviour
{
    [Header("Race Settings")]
    [Networked] public int LapsToComplete { get; set; } = 2;
    [Networked] public NetworkBool RaceStarted { get; set; }
    [Networked] public NetworkBool RaceFinished { get; set; }
    [Networked] public TickTimer CountdownTimer { get; set; }

    [Header("UI References")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;
    public GameObject resultsPanel;
    public Transform resultsContainer;
    public GameObject resultItemPrefab;

    [Header("Race State")]
    [Networked, Capacity(16)]
    public NetworkLinkedList<NetworkString<_16>> FinishOrder { get; }
    [Networked, Capacity(16)]
    public NetworkArray<float> FinishTimes { get; }

    private float raceStartTime;
    private List<NetworkPlayer> finishedPlayers = new List<NetworkPlayer>();

    public static RaceManager Instance { get; private set; }

    private void Awake()
    {
        // ВИПРАВЛЕННЯ: Жорстка перевірка синглтона
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Знайдено дублікат RaceManager. Видаляємо старий.");
            // Не використовуй DontDestroyOnLoad для RaceManager, 
            // якщо він є частиною сцени "Online".
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void Spawned()
    {
        // Скидаємо панелі при спавні в мережі
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        if (RaceStarted && !RaceFinished)
        {
            CheckRaceCompletion();
        }
    }

    #region Race Start

    public void StartCountdown()
    {
        // Тільки Host може запускати таймер
        if (Object.HasStateAuthority)
        {
            LapsToComplete = 2;
            CountdownTimer = TickTimer.CreateFromSeconds(Runner, 4f);

            // Замість Coroutine краще використовувати RPC, 
            // щоб візуал був у всіх, але логіка — на сервері
            RpcStartCountdownVisual();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcStartCountdownVisual()
    {
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        if (countdownPanel != null) countdownPanel.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null) countdownText.text = "GO!";

        if (Object.HasStateAuthority)
        {
            RaceStarted = true;
            raceStartTime = Time.time;
        }

        yield return new WaitForSeconds(0.5f);
        if (countdownPanel != null) countdownPanel.SetActive(false);
    }

    #endregion

    #region Race Progress (Fixed)

    public void OnPlayerFinishedRace(NetworkPlayer player)
    {
        if (!Object.HasStateAuthority) return;

        if (!finishedPlayers.Contains(player))
        {
            finishedPlayers.Add(player);
            player.HasFinishedRace = true;
            player.FinishTime = Time.time - raceStartTime;

            // Оновлюємо мережеві списки
            FinishOrder.Add(player.NickName);
            FinishTimes.Set(finishedPlayers.Count - 1, player.FinishTime);

            // Оновлюємо лідерборд (якщо він є)
            RpcRefreshLeaderboard();

            var allPlayers = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
            if (finishedPlayers.Count >= allPlayers.Length)
            {
                EndRace();
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcRefreshLeaderboard()
    {
        // Якщо у тебе є UI лідерборду в реальному часі
        GameObject.FindGameObjectWithTag("Leaderboard")?.GetComponent<LeaderboardUIHandler>()?.ReloadList();
    }

    private void CheckRaceCompletion()
    {
        var allPlayers = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        int finishedCount = allPlayers.Count(p => p.HasFinishedRace);

        if (finishedCount > 0 && (finishedCount >= allPlayers.Length || (Time.time - raceStartTime) > 600))
        {
            EndRace();
        }
    }

    private void EndRace()
    {
        if (Object.HasStateAuthority)
        {
            RaceFinished = true;
            RpcShowResults();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcShowResults()
    {
        ShowResultsPanel();
    }

    #endregion

    #region Results UI

    private void ShowResultsPanel()
    {
        if (resultsPanel == null) return;
        resultsPanel.SetActive(true);

        if (resultsContainer != null)
        {
            foreach (Transform child in resultsContainer) Destroy(child.gameObject);
        }

        var sortedPlayers = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None)
            .OrderBy(p => p.HasFinishedRace ? 0 : 1)
            .ThenBy(p => p.FinishTime)
            .ToList();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            var player = sortedPlayers[i];
            if (resultItemPrefab != null && resultsContainer != null)
            {
                var item = Instantiate(resultItemPrefab, resultsContainer);
                var texts = item.GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"{i + 1}.";
                    texts[1].text = player.NickName.ToString();
                    texts[2].text = player.HasFinishedRace ? FormatTime(player.FinishTime) : "DNF";
                }
            }
        }
    }

    private string FormatTime(float time)
    {
        int min = (int)(time / 60);
        int sec = (int)(time % 60);
        int ms = (int)((time * 1000) % 1000);
        return $"{min:00}:{sec:00}.{ms:000}";
    }
    #endregion

    #region Utility
    public int GetPlayerPosition(NetworkPlayer player)
    {
        var allPlayers = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None)
            .OrderByDescending(p => p.GetCheckpointsPassed())
            .ThenBy(p => p.GetLastCheckpointTime())
            .ToList();

        return allPlayers.IndexOf(player) + 1;
    }
    #endregion
}