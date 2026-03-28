using TMPro;
using UnityEngine;

public class SetLeaderboardItemInfo : MonoBehaviour
{
    public TMP_Text positionText;
    public TMP_Text drivetNameText;

    public void SetPositionText(string newPosition)
    {
        positionText.text = newPosition;
    }

    public void SetDriverNameText(string driverName)
    {
        drivetNameText.text = driverName;
    }
}
