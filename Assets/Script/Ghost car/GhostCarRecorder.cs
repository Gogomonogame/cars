using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GhostCarRecorder : MonoBehaviour
{
    public Transform carSprite;
    public GameObject ghostCarPlaybackPrefab;

    GhostCarData ghostCarData = new GhostCarData();

    bool isRecording = true;

    Rigidbody2D carRigidbody2D;
    CarInputHandler carInputHandler;

    private void Awake()
    {
        carRigidbody2D = GetComponent<Rigidbody2D>();
        carInputHandler = GetComponent<CarInputHandler>();
    }

    private void Start()
    {
        GameObject ghostCar = Instantiate(ghostCarPlaybackPrefab);

        ghostCar.GetComponent<GhostCarPlayback>().LoadData(carInputHandler.playerNumber);

        StartCoroutine(RecordCarPositionCO());
        StartCoroutine(SaveCarPositionCO());
    }
    
    IEnumerator RecordCarPositionCO()
    {
        while (isRecording)
        {
            if(carSprite != null)
                ghostCarData.AddDataItem(new GhostCarDataListItem(carRigidbody2D.position, carRigidbody2D.rotation, carSprite.localScale,Time.timeSinceLevelLoad));
            yield return new WaitForSeconds(0.15f);
        }

    }

    IEnumerator SaveCarPositionCO()
    {
           
        yield return new WaitForSeconds(5);
        SaveData();
        
            
    }

    void SaveData()
    {
        string jsonEncodedData = JsonUtility.ToJson(ghostCarData);

        print($"Saved ghost data {jsonEncodedData}");

        if(carInputHandler != null)
        {
            PlayerPrefs.SetString($"{SceneManager.GetActiveScene().name}_{carInputHandler.playerNumber}_ghost", jsonEncodedData);
            PlayerPrefs.Save();
        }

        isRecording = false;
    }

}
