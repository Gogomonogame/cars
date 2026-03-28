using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEngine;

public class PositionHandler : MonoBehaviour
{
    LeaderboardUIHandler leaderboardUIHandler;

    public List<CarLapCounter> carLapCounters = new List<CarLapCounter>();

    private void Awake()
    {
        CarLapCounter[] carLapCounterArray = FindObjectsOfType<CarLapCounter>();
        carLapCounters = carLapCounterArray.ToList<CarLapCounter>();
        foreach(CarLapCounter lapCounter in carLapCounters)
        {
            lapCounter.OnPassCheckpoint += OnPassCheckpoint;
        }
        leaderboardUIHandler = FindObjectOfType<LeaderboardUIHandler>();
    }
    private void Start()
    {
        leaderboardUIHandler.UpdateList(carLapCounters);
    }

    void OnPassCheckpoint(CarLapCounter carLapCounter)
    {
        //Sort by position on how many checkpoints they passed and then by time, lost in them
        carLapCounters = carLapCounters.OrderByDescending(s => s.GetNumberOfCheckpointsPassed()).ThenBy(s => s.GetTimeAtLastCheckpoint()).ToList();
        int carPosition = carLapCounters.IndexOf(carLapCounter) + 1;
        carLapCounter.SetCarPosition(carPosition);
        leaderboardUIHandler.UpdateList(carLapCounters);
    }
}
