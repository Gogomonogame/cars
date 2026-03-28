using UnityEngine;

public class SpawnCars : MonoBehaviour
{
    private void Start()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        CarData[] carDatas = Resources.LoadAll<CarData>("CarData/");

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform spawnPoint = spawnPoints[i].transform;

            int playerSelectedCarID = PlayerPrefs.GetInt($"P{i + 1}SelectedCarID");

            foreach (CarData carData in carDatas)
            {
                if(carData.CarUniqueID == playerSelectedCarID)
                {
                    GameObject playerCar = Instantiate(carData.CarPrefab, spawnPoint.position, spawnPoint.rotation);
                    playerCar.GetComponent<CarInputHandler>().playerNumber = i + 1;
                    break;
                }
            }
        }
    }
}
