using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
public class NetworkSpawnCar : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPlayer playerPrefab;

    CarInputHandler localCarInputHandler;

    Vector3 GetRandomSpawnPoint()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        print(spawnPoints);
        if (spawnPoints.Length == 0) return Vector3.zero;
        else return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position;
    }


    public void OnConnectedToServer(NetworkRunner runner)
    {
        if (runner.Topology == SimulationConfig.Topologies.Shared)
        {
            print("OnConnectedToServer, starting player prefab as local player");

            runner.Spawn(playerPrefab, GetRandomSpawnPoint(), Quaternion.identity, runner.LocalPlayer);
        }
        else
        {
            print("OnConnectedToServer");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            print("OnPlayerJoined, we are the server. Spawning player");
            runner.Spawn(playerPrefab, GetRandomSpawnPoint(), Quaternion.identity, player);
        }
        else
        {
            print("OnPlayerJoined");

        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (localCarInputHandler == null && NetworkPlayer.Local != null)
        {
            localCarInputHandler = NetworkPlayer.Local.GetComponent<CarInputHandler>();
        }
        if(localCarInputHandler != null)
        {
            input.Set(localCarInputHandler.GetNetworkInput());
        }
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)  { print("OnConnectFailed"); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { print("OnConnectRequest"); }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { print("OnDisconnectedFromServer"); }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { print("OnShutdown"); }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
