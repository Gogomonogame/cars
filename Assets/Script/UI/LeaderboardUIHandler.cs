using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LeaderboardUIHandler : MonoBehaviour
{
    public GameObject leaderboardItemPrefab;
    SetLeaderboardItemInfo[] setLeaderboardItemInfo;

    public void ReloadList()
    {
        VerticalLayoutGroup leaderboardLayoutGroup = GetComponentInChildren<VerticalLayoutGroup>();

        // Очищення старих елементів
        foreach (Transform child in leaderboardLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        // Знаходимо всі машини
        CarLapCounter[] carLapCounterArray = FindObjectsOfType<CarLapCounter>();
        setLeaderboardItemInfo = new SetLeaderboardItemInfo[carLapCounterArray.Length];

        for (int i = 0; i < carLapCounterArray.Length; i++)
        {
            // Підписка на подію чекпоінту
            carLapCounterArray[i].OnPassCheckpoint -= OnCarPassCheckpoint;
            carLapCounterArray[i].OnPassCheckpoint += OnCarPassCheckpoint;

            GameObject item = Instantiate(leaderboardItemPrefab, leaderboardLayoutGroup.transform);
            setLeaderboardItemInfo[i] = item.GetComponent<SetLeaderboardItemInfo>();

            setLeaderboardItemInfo[i].SetPositionText($"{i + 1}.");
            setLeaderboardItemInfo[i].SetDriverNameText(carLapCounterArray[i].gameObject.name);
        }
    }

    // Логіка оновлення при проїзді чекпоінту
    void OnCarPassCheckpoint(CarLapCounter car)
    {
        CarLapCounter[] allCars = FindObjectsOfType<CarLapCounter>();

        // Сортування: спочатку за кількістю чекпоінтів, потім за часом (хто раніше перетнув)
        var sortedCars = allCars
            .OrderByDescending(c => c.GetNumberOfCheckpointsPassed())
            .ThenBy(c => c.GetTimeAtLastCheckpoint())
            .ToList();

        for (int i = 0; i < sortedCars.Count; i++)
        {
            // Передаємо машині її нове місце
            sortedCars[i].SetCarPosition(i + 1);

            // Оновлюємо текст у таблиці лідерів
            if (i < setLeaderboardItemInfo.Length)
            {
                setLeaderboardItemInfo[i].SetDriverNameText(sortedCars[i].gameObject.name);
            }
        }
    }

    // Метод для оновлення імен (якщо потрібно)
    public void UpdateList(List<CarLapCounter> lapCounters)
    {
        for (int i = 0; i < lapCounters.Count; i++)
        {
            if (i < setLeaderboardItemInfo.Length)
                setLeaderboardItemInfo[i].SetDriverNameText(lapCounters[i].gameObject.name);
        }
    }
}