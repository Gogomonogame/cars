using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CarLapCounter : MonoBehaviour
{
    public TMP_Text carPositionText;

    int passedCheckPointNumber = 0;
    float timeLastPassedCheckPoint = 0;
    int numberOfPassedCheckpoints = 0;
    int lapsCompleted = 0;
    const int lapsToComplete = 2;
    bool isRaceCompleted = false;
    int carPosition = 0;

    Coroutine hideRoutine;

    public event Action<CarLapCounter> OnPassCheckpoint;

    public void SetCarPosition(int position)
    {
        carPosition = position;
        if (carPositionText.gameObject.activeSelf)
        {
            UpdatePositionText();
        }
    }

    public int GetNumberOfCheckpointsPassed() => numberOfPassedCheckpoints;
    public float GetTimeAtLastCheckpoint() => timeLastPassedCheckPoint;

    private void UpdatePositionText()
    {
        string suffix = "th";
        if (carPosition == 1) suffix = "st";
        else if (carPosition == 2) suffix = "nd";
        else if (carPosition == 3) suffix = "rd";

        carPositionText.text = $"{carPosition}-{suffix}";
    }

    IEnumerator ShowPositionCO(float delay)
    {
        UpdatePositionText();
        carPositionText.gameObject.SetActive(true);
        yield return new WaitForSeconds(delay);
        carPositionText.gameObject.SetActive(false);
        hideRoutine = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("CheckPoint"))
        {
            if (isRaceCompleted) return;

            CheckPoint checkPoint = collision.GetComponent<CheckPoint>();
            if (passedCheckPointNumber + 1 == checkPoint.checkPointNumber)
            {
                passedCheckPointNumber = checkPoint.checkPointNumber;
                numberOfPassedCheckpoints++;
                timeLastPassedCheckPoint = Time.time;

                if (checkPoint.isFinishLine)
                {
                    passedCheckPointNumber = 0;
                    lapsCompleted++;
                    if (lapsCompleted >= lapsToComplete) isRaceCompleted = true;
                }

                OnPassCheckpoint?.Invoke(this);

                if (hideRoutine != null) StopCoroutine(hideRoutine);
                hideRoutine = StartCoroutine(ShowPositionCO(isRaceCompleted ? 100f : 1.5f));
            }
        }
    }
}