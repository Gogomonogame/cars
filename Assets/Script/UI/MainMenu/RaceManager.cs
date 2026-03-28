using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceManager : NetworkBehaviour
{
    [Header("Race Settings")]
    [Networked] public int LapsToComplete { get; set; } = 2;
    [Networked] public NetworkBool RaceStarted { get; set; }
    [Networked] public NetworkBool RaceFinished { get; set; }
    [Networked] public TickTimer CountdownTimer { get; set; }
    [Networked] public int CountdownValue { get; set; }

    [Header("UI References")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;
    public GameObject resultsPanel;
    public Transform resultsContainer;
    public GameObject resultItemPrefab;
    public TMP_Text raceStatusText;

    [Header("Race State")]
    [Networked, Capacity(16)]
    public NetworkLinkedList<NetworkString<_16>> FinishOrder { get; }

    [Networked, Capacity(16)]
    public NetworkArray<float> FinishTimes { get; }

    private float raceStartTime;
    private List<NetworkPlayer> finishedPlayers = new List<NetworkPlayer>();

    public static RaceManager Instance { get; private set; }

    public override void Spawned()
    {
        Instance = this;

        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        if (resultsPanel != null)
            resultsPanel.SetActive(false);
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
        if (Object.HasStateAuthority)
        {
            LapsToComplete = 2; // Default, can be set from lobby
            CountdownValue = 3;
            CountdownTimer = TickTimer.CreateFromSeconds(Runner, 4f);
            StartCoroutine(CountdownCoroutine());
        }
    }

    private IEnumerator CountdownCoroutine()
    {
        if (countdownPanel != null)
            countdownPanel.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();

            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
            countdownText.text = "GO!";

        yield return new WaitForSeconds(0.5f);

        // Start race
        RaceStarted = true;
        raceStartTime = Time.time;

        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        Debug.Log("Race Started!");
    }

    #endregion

    #region Race Progress

    public void OnPlayerFinishedRace(NetworkPlayer player)
    {
        if (!Object.HasStateAuthority) return;

        if (!finishedPlayers.Contains(player))
        {
            finishedPlayers.Add(player);
            player.HasFinishedRace = true;
            player.FinishTime = Time.time - raceStartTime;

            // Update finish order
            FinishOrder.Add(player.NickName);
            FinishTimes.Set(finishedPlayers.Count - 1, player.FinishTime);

            Debug.Log($"{player.NickName} finished at position {finishedPlayers.Count}");

            // Check if all players finished
            var allPlayers = FindObjectsOfType<NetworkPlayer>();
            if (finishedPlayers.Count >= allPlayers.Length)
            {
                EndRace();
            }
        }
    }

    private void CheckRaceCompletion()
    {
        // Check if all active players have finished
        var allPlayers = FindObjectsOfType<NetworkPlayer>();
        int finishedCount = 0;

        foreach (var player in allPlayers)
        {
            if (player.HasFinishedRace)
                finishedCount++;
        }

        // If at least one player finished and 30 seconds passed, or all finished
        if (finishedCount > 0 && (finishedCount >= allPlayers.Length || (Time.time - raceStartTime) > 300))
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

    #region Results

    private void ShowResultsPanel()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);

            // Clear existing results
            if (resultsContainer != null)
            {
                foreach (Transform child in resultsContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // Show results
            var allPlayers = FindObjectsOfType<NetworkPlayer>()
                .OrderBy(p => p.HasFinishedRace ? 0 : 1)
                .ThenBy(p => p.FinishTime)
                .ToList();

            for (int i = 0; i < allPlayers.Count; i++)
            {
                var player = allPlayers[i];
                if (resultItemPrefab != null && resultsContainer != null)
                {
                    var item = Instantiate(resultItemPrefab, resultsContainer);
                    var texts = item.GetComponentsInChildren<TMP_Text>();
                    if (texts.Length >= 3)
                    {
                        texts[0].text = $"{i + 1}.";
                        texts[1].text = player.NickName.Value;
                        texts[2].text = player.HasFinishedRace ?
                            FormatTime(player.FinishTime) : "DNF";
                    }
                }
            }
        }
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 1000) % 1000);
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }

    #endregion

    #region Utility

    public int GetPlayerPosition(NetworkPlayer player)
    {
        var allPlayers = FindObjectsOfType<NetworkPlayer>()
            .OrderByDescending(p => p.GetCheckpointsPassed())
            .ThenBy(p => p.GetLastCheckpointTime())
            .ToList();

        return allPlayers.IndexOf(player) + 1;
    }

    public bool CanRaceStart()
    {
        var allPlayers = FindObjectsOfType<NetworkPlayer>();
        return allPlayers.Length >= 1; // At least 1 player to start
    }

    #endregion
}