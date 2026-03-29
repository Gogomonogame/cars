using Fusion;
using UnityEngine;
using System.Collections;

public class NetworkRunnerHandler : MonoBehaviour
{
    private NetworkRunner _networkRunner;

    private void Start()
    {
        _networkRunner = FindObjectOfType<NetworkRunner>();

        if (_networkRunner != null && _networkRunner.IsRunning)
        {
            Debug.Log("NetworkRunnerHandler: [INFO] Runner connected.");

            // Allow this Runner to provide input (Critical for client control)
            _networkRunner.ProvideInput = true;

            var spawner = GetComponent<NetworkSpawnCar>();
            if (spawner != null)
            {
                _networkRunner.AddCallbacks(spawner);
                StartCoroutine(WaitAndSpawn(spawner));
            }
        }
    }

    private IEnumerator WaitAndSpawn(NetworkSpawnCar spawner)
    {
        yield return new WaitForSeconds(0.5f);
        // Only spawn if local player doesn't have a car yet
        if (_networkRunner != null && _networkRunner.GetPlayerObject(_networkRunner.LocalPlayer) == null)
        {
            spawner.OnSceneLoadDone(_networkRunner);
        }
    }
}