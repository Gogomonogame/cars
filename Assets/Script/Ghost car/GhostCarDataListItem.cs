using System;
using UnityEngine;

[Serializable]
public class GhostCarDataListItem : ISerializationCallbackReceiver
{
    [NonSerialized]
    public Vector2 position = Vector2.zero;

    [NonSerialized]
    public float rotationZ = 0;

    [NonSerialized]
    public float timeSinceLevelLoaded = 0;

    [NonSerialized]
    public Vector3 localScale = Vector3.one;

    //To preserve size we round off calues of floats
    [SerializeField]
    int x = 0;//Vector x

    [SerializeField]
    int y = 0;//Vector y

    [SerializeField]
    int r = 0;//rotation

    [SerializeField]
    int t = 0;//time

    [SerializeField]
    int s = 0;//scale

    public GhostCarDataListItem(Vector2 _position, float _rotation, Vector3 _localScale, float _timeSinceLevelLoaded)
    {
        position = _position;
        rotationZ = _rotation;
        localScale = _localScale;
        timeSinceLevelLoaded = _timeSinceLevelLoaded;
    }

    public void OnBeforeSerialize()
    {
        t = (int)(timeSinceLevelLoaded * 1000.0f);

        x = (int)(position.x * 1000.0f);
        y = (int)(position.y * 1000.0f);

        s = (int)(localScale.x * 1000.0f);

        r = Mathf.RoundToInt(rotationZ);
    }

    public void OnAfterDeserialize()
    {
        timeSinceLevelLoaded = t / 1000.0f;
        position.x = x / 1000.0f;
        position.y = y / 1000.0f;
        localScale = new Vector3(s / 1000.0f, s / 1000.0f, s / 1000.0f);

        rotationZ = r;
    }

    
}
