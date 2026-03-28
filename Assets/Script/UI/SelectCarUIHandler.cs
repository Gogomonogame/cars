using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectCarUIHandler : MonoBehaviour
{
    [Header("Car prefab")]
    public GameObject carPrefab;

    [Header("Spawn on")]
    public Transform spawnOnTransform;

    bool isChangingCar = false;

    CarUIHandler carUIHandler;

    CarData[] carDatas;

    int selectedIndex = 0;
    private void Start()
    {
        carDatas = Resources.LoadAll<CarData>("CarData");
        StartCoroutine(SpawnCarCO(true));
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            OnPreviousCar();
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            OnNextCar();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            OnSelectCar();
        }
    }

    public void OnPreviousCar()
    {
        if (isChangingCar) return;
        selectedIndex--;
        if (selectedIndex < 1)
            selectedIndex = carDatas.Length - 1;
        StartCoroutine(SpawnCarCO(true));
    }

    public void OnNextCar()
    {
        if (isChangingCar) return;
        selectedIndex++;
        if(selectedIndex > carDatas.Length - 1)
            selectedIndex = 0;
        StartCoroutine(SpawnCarCO(false));
    }

    public void OnSelectCar()
    {
        PlayerPrefs.SetInt("P1SelectedCarID", carDatas[selectedIndex].CarUniqueID);
        PlayerPrefs.SetInt("P2SelectedCarID", carDatas[selectedIndex].CarUniqueID);
        PlayerPrefs.SetInt("P3SelectedCarID", carDatas[selectedIndex].CarUniqueID);
        PlayerPrefs.SetInt("P4SelectedCarID", carDatas[selectedIndex].CarUniqueID);

        PlayerPrefs.Save();

        SceneManager.LoadScene("SpawnCar");
    }

    IEnumerator SpawnCarCO(bool isCarAppearingOnRightSide)
    {
        isChangingCar = true;

        if(carUIHandler != null)
            carUIHandler.StartCarExitAnimation(!isCarAppearingOnRightSide);

        GameObject insatantiatedCar = Instantiate(carPrefab,spawnOnTransform);

        carUIHandler = insatantiatedCar.GetComponent<CarUIHandler>();
        carUIHandler.SetupCar(carDatas[selectedIndex]);
        carUIHandler.StartCarEntranceAnimation(isCarAppearingOnRightSide);

        yield return new WaitForSeconds(0.4f);

        isChangingCar = false;
    }
}
