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
using static Unity.Collections.Unicode;

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

        // Прив'язка кнопок
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

        await StartGame(GameMode.Shared, roomName); // Shared mode зручніший для дипломів
    }

    public async void OnJoinRoomClicked()
    {
        SaveNickName();
        ShowRoomList();
        // У Fusion для отримання списку сесій треба підключитися до лобі
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
        // IsSharedModeMasterClient — це property, без дужок!
        if (networkRunner.IsServer || networkRunner.IsSharedModeMasterClient)
        {
            // У Fusion завантаження сцени робиться так:
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
        // Перевірка SceneManager
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
        this.sessionList.Clear();
        foreach (var session in sessionList) this.sessionList[session.Name] = session;
        UpdateRoomListUI();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) => UpdatePlayersListUI();
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => UpdatePlayersListUI();

    // Виправлені сигнатури для Unity Messages
    public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("Connected"); }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { ShowMainMenu(); }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) => ShowMainMenu();

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    // Решта обов'язкових методів інтерфейсу (порожні)
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
        if (roomListContainer == null) return;
        foreach (Transform child in roomListContainer) Destroy(child.gameObject);

        foreach (var session in sessionList.Values)
        {
            var item = Instantiate(roomListItemPrefab, roomListContainer);
            item.GetComponentInChildren<TMP_Text>().text = $"{session.Name} ({session.PlayerCount}/{session.MaxPlayers})";
            item.GetComponent<Button>().onClick.AddListener(() => OnJoinSpecificRoom(session.Name));
        }
    }

    private void UpdatePlayersListUI()
    {
        if (playersListContainer == null) return;
        foreach (Transform child in playersListContainer) Destroy(child.gameObject);

        // Використовуємо актуальний метод FindObjectsByType
        var players = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            var item = Instantiate(playerListItemPrefab, playersListContainer);
            // Переконайся, що в NetworkPlayer є поле NickName (тип NetworkString)
            item.GetComponentInChildren<TMP_Text>().text = player.NickName.ToString();
        }
    }

    
}