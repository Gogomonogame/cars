using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using System.Linq;

public class NetworkSpawnCar : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefabs")]
    public NetworkObject playerPrefab;
    CarInputHandler localCarInputHandler;
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        // Check if the local client has authority to spawn (Server, Host, or Master in Shared Mode)
        bool canSpawn = runner.IsServer || runner.IsSharedModeMasterClient;

        if (canSpawn)
        {
            Debug.Log($"[SPAWNER] Scene Load Done. Active Players: {runner.ActivePlayers.Count()}");
            foreach (var player in runner.ActivePlayers)
            {
                SpawnPlayer(runner, player);
            }
        }
        else
        {
            Debug.Log("[SPAWNER] Authority check failed: Local client is not the Server/Master.");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer || runner.IsSharedModeMasterClient)
        {
            Debug.Log($"[SPAWNER] Player {player} joined. Initiating spawn...");
            SpawnPlayer(runner, player);
        }
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        // 1. Check if the player already has an object associated with them
        if (runner.GetPlayerObject(player) != null)
        {
            Debug.Log($"[SPAWNER] Player {player} already has a car. Skipping spawn.");
            return;
        }

        // 2. Extra safety: Check if a car for this player already exists in the scene
        // This prevents the "3 cars for 2 players" issue
        NetworkPlayer[] existingPlayers = FindObjectsOfType<NetworkPlayer>();
        foreach (var p in existingPlayers)
        {
            if (p.Object.InputAuthority == player)
            {
                Debug.Log($"[SPAWNER] Found existing object for player {player}. Linking...");
                runner.SetPlayerObject(player, p.Object);
                return;
            }
        }

        // 3. If everything is clear, spawn the car
        Vector3 spawnPos = GetRandomSpawnPoint();
        Debug.Log($"[SPAWNER] Spawning NEW car for player {player}");
        var playerObj = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
        runner.SetPlayerObject(player, playerObj);
    }

    private Vector3 GetRandomSpawnPoint()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position;
        }

        Debug.LogError("[SPAWNER] ERROR: No objects found with tag 'SpawnPoint'!");
        return Vector3.zero;
    }

    // --- Required INetworkRunnerCallbacks Implementation ---

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // «находимо локального гравц€ (ми додали це в NetworkPlayer.Local)
        if (NetworkPlayer.Local != null)
        {
            var handler = NetworkPlayer.Local.carInputHandler;
            if (handler != null)
            {
                input.Set(handler.GetNetworkInput());
            }
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) => Debug.Log($"[FUSION] Shutdown: {shutdownReason}");
    public void OnConnectedToServer(NetworkRunner runner) => Debug.Log("[FUSION] Connected to server.");
    public void OnDisconnectedFromServer(NetworkRunner runner) => Debug.Log("[FUSION] Disconnected.");
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) => Debug.Log($"[FUSION] Connect failed: {reason}");
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => Debug.Log($"[FUSION] Player {player} left.");

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}