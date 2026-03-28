using Fusion;
using UnityEngine;
using TMPro;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Player Info")]
    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> NickName { get; set; }

    [Networked]
    public int CarSelectionId { get; set; }

    [Networked]
    public NetworkBool IsReady { get; set; }

    [Networked]
    public NetworkBool HasFinishedRace { get; set; }

    [Networked]
    public float FinishTime { get; set; }

    public static NetworkPlayer Local { get; private set; }

    [Header("Components")]
    public CarController carController;
    public CarInputHandler carInputHandler;
    public CarLapCounter carLapCounter;

    [Header("UI")]
    public TMP_Text playerNameText;

    bool isInitialized = false;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;

            // Send local player's nickname to the network
            string savedNickName = PlayerPrefs.GetString("PlayerNickName", "Player");
            RpcSetNickName(savedNickName);

            // Send car selection
            int selectedCarId = PlayerPrefs.GetInt("P1SelectedCarID", 0);
            RpcSetCarSelection(selectedCarId);
        }

        // Set up the transform
        transform.parent = null;

        DontDestroyOnLoad(gameObject);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcSetNickName(string nickName)
    {
        NickName = nickName;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcSetCarSelection(int carId)
    {
        CarSelectionId = carId;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcSetReady(bool isReady)
    {
        IsReady = isReady;
    }

    static void OnNickNameChanged(Changed<NetworkPlayer> changed)
    {
        // Update UI when nickname changes
    }

    public void SetFinishResult(int position, float time)
    {
        if (!HasFinishedRace)
        {
            HasFinishedRace = true;
            FinishTime = time;
        }
    }

    public int GetCheckpointsPassed()
    {
        if (carLapCounter != null)
            return carLapCounter.GetNumberOfCheckpointsPassed();
        return 0;
    }

    public float GetLastCheckpointTime()
    {
        if (carLapCounter != null)
            return carLapCounter.GetTimeAtLastCheckpoint();
        return 0;
    }
}