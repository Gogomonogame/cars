using UnityEngine;

[CreateAssetMenu(fileName = "New Car Data", menuName = "Car Data", order = 51)]
public class CarData : ScriptableObject
{
    [SerializeField]
    private int carUniqueId = 0;

    [SerializeField]
    private Sprite carUISprite;

    [SerializeField]
    private GameObject carPrefab;

    public int CarUniqueID
    {
        get { return carUniqueId; }
    }

    public Sprite CarUISprite
    {
        get { return carUISprite; }
    }

    public GameObject CarPrefab
    {
        get { return carPrefab; }
    }
}
