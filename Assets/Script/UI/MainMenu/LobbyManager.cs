using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("UI References")]
    public GameObject mainMenuPanel;
    public GameObject lobbyPanel;
    public GameObject roomListPanel;

    [Header("Main Menu UI")]
    public TMP_InputField nickNameInputField;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button quickMatchButton;

    [Header("Lobby UI")]
    public TMP_InputField roomNameInputField;
    public Button startGameButton;
    public Button leaveLobbyButton;
    public Transform playersListContainer;
    public GameObject playerListItemPrefab;
    public TMP_Text roomNameText;

    [Header("Room List UI")]
    public Transform roomListContainer;
    public GameObject roomListItemPrefab;
    public Button refreshRoomsButton;
    public Button backButton;

    [Header("Game Settings")]
    public string gameSceneName = "Online";
    public int maxPlayers = 4;

    private NetworkRunner networkRunner;
    private Dictionary<string, SessionInfo> sessionList = new Dictionary<string, SessionInfo>();

    public static LobbyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        string savedNick = PlayerPrefs.GetString("PlayerNickName", "");
        if (!string.IsNullOrEmpty(savedNick)) nickNameInputField.text = savedNick;

        createRoomButton?.onClick.AddListener(OnCreateRoomClicked);
        joinRoomButton?.onClick.AddListener(OnJoinRoomClicked);
        quickMatchButton?.onClick.AddListener(OnQuickMatchClicked);
        startGameButton?.onClick.AddListener(OnStartGameClicked);
        leaveLobbyButton?.onClick.AddListener(OnLeaveLobbyClicked);
        refreshRoomsButton?.onClick.AddListener(OnRefreshRoomsClicked);
        backButton?.onClick.AddListener(OnBackClicked);

        networkRunner = GetComponent<NetworkRunner>() ?? gameObject.AddComponent<NetworkRunner>();
        networkRunner.AddCallbacks(this);

        ShowMainMenu();
    }

    #region UI Methods
    public void ShowMainMenu()
    {
        mainMenuPanel?.SetActive(true);
        lobbyPanel?.SetActive(false);
        roomListPanel?.SetActive(false);
    }
    public void ShowLobby()
    {
        mainMenuPanel?.SetActive(false);
        lobbyPanel?.SetActive(true);
        roomListPanel?.SetActive(false);
    }
    public void ShowRoomList()
    {
        mainMenuPanel?.SetActive(false);
        lobbyPanel?.SetActive(false);
        roomListPanel?.SetActive(true);
    }

    private void SaveNickName()
    {
        if (nickNameInputField != null && !string.IsNullOrEmpty(nickNameInputField.text))
        {
            PlayerPrefs.SetString("PlayerNickName", nickNameInputField.text);
            PlayerPrefs.Save();
        }
    }
    #endregion

    #region Button Handlers
    public async void OnCreateRoomClicked()
    {
        SaveNickName();
        string roomName = string.IsNullOrEmpty(roomNameInputField.text)
            ? "Room_" + UnityEngine.Random.Range(1000, 9999)
            : roomNameInputField.text;

        await StartGame(GameMode.Shared, roomName);
    }

    public async void OnJoinRoomClicked()
    {
        SaveNickName();
        ShowRoomList();
        await networkRunner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    public async void OnQuickMatchClicked()
    {
        SaveNickName();
        await StartGame(GameMode.Shared, null);
    }

    public async void OnJoinSpecificRoom(string roomName)
    {
        await StartGame(GameMode.Shared, roomName);
    }

    public void OnStartGameClicked()
    {
        if (networkRunner.IsServer || networkRunner.IsSharedModeMasterClient)
        {
            networkRunner.SetActiveScene(gameSceneName);
        }
    }

    public async void OnLeaveLobbyClicked()
    {
        await networkRunner.Shutdown();
        ShowMainMenu();
    }

    public void OnRefreshRoomsClicked() => UpdateRoomListUI();

    public async void OnBackClicked()
    {
        if (networkRunner.IsRunning) await networkRunner.Shutdown();
        ShowMainMenu();
    }
    #endregion

    private async Task StartGame(GameMode mode, string sessionName)
    {
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            PlayerCount = maxPlayers,
            SceneManager = sceneManager
        });

        if (result.Ok)
        {
            ShowLobby();
            if (roomNameText != null) roomNameText.text = networkRunner.SessionInfo.Name;
        }
    }

    #region INetworkRunnerCallbacks
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (this == null || !gameObject.activeInHierarchy) return; // ﬂÍ˘Ó Ó·'∫ÍÚ ‚Ë‰‡ÎÂÌÓ ‡·Ó ‚ËÏÍÌÂÌÓ

        this.sessionList.Clear();
        foreach (var session in sessionList) this.sessionList[session.Name] = session;
        UpdateRoomListUI();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) => UpdatePlayersListUI();
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => UpdatePlayersListUI();
    public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("Connected"); }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { ShowMainMenu(); }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) => ShowMainMenu();

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    #endregion

    private void UpdateRoomListUI()
    {
        if (roomListContainer == null || roomListItemPrefab == null) return;

        foreach (Transform child in roomListContainer) Destroy(child.gameObject);

        foreach (var session in sessionList.Values)
        {
            // œÂÂ‚≥ˇ∫ÏÓ, ˜Ë ÒÂÒ≥ˇ ‡ÍÚË‚Ì‡ Ú‡ ˜Ë ∫ ‚≥Î¸Ì≥ Ï≥Òˆˇ (ÓÔˆ≥ÓÌ‡Î¸ÌÓ)
            if (session.IsVisible && session.IsOpen)
            {
                GameObject item = Instantiate(roomListItemPrefab, roomListContainer);

                // ¡≈«œ≈◊Õ»… œŒÿ”  “≈ —“”
                var textComponent = item.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = $"{session.Name} ({session.PlayerCount}/{session.MaxPlayers})";
                }

                // ¡≈«œ≈◊Õ»… œŒÿ”   ÕŒœ »
                var btn = item.GetComponent<Button>();
                if (btn != null)
                {
                    string sessionName = session.Name; // ÀÓÍ‡Î¸Ì‡ ÁÏ≥ÌÌ‡ ‰Îˇ Á‡ÏËÍ‡ÌÌˇ
                    btn.onClick.AddListener(() => OnJoinSpecificRoom(sessionName));
                }
            }
        }
    }

    private void UpdatePlayersListUI()
    {
        if (playersListContainer == null) return;
        foreach (Transform child in playersListContainer) Destroy(child.gameObject);

        var players = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            var item = Instantiate(playerListItemPrefab, playersListContainer);
            item.GetComponentInChildren<TMP_Text>().text = player.NickName.ToString();
        }
    }

    
}