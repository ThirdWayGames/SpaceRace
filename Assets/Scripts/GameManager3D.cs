using Assets.Scripts.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager3D : Photon.PunBehaviour
{
    private static GameManager3D GameManager;

    public static GameManager3D instance
    {
        get {
            if (!GameManager)
            {
                GameManager = FindObjectOfType<GameManager3D>() as GameManager3D;

                if (!GameManager)
                {
                    Debug.LogError("There is no GameManager found in the scene");
                }
            }

            return GameManager;
        }
    }

    public ScriptableObject PrevGameCompletionTime;

    public GameObject RedMarinePrefab;

    public GameObject BlueMarinePrefab;

    public List<bool> GameEndConditions;

    public RectTransform GameStatePanel;

    public RectTransform ConsolePanel;

    public TeleportController[] SpawnLocations;

    public List<AccessConsole3D> AccessConsoles;

    public Button ReturnToLobbyBtn;

    public bool IsDebug = false;

    public int PlayerTeamOverride = 0;

    public const string MatchPhaseKey = "MatchPhase";

    public const string WinningTeamKey = "WinningTeam";

    public const string MatchPhaseEnded = "Ended";

    bool matchEnded = false;

    bool winPanelShown = false;

    bool lobbyReturnScheduled = false;

    bool teamAssignmentRequested = false;

    private bool IsConsoleShowing = false;

    private int ConsoleMessageBacklog = 30;

    private int ConsoleMessageCount = 0;

    public GameObject GetMarinePrefab()
    {
        return GetTeamId() == 1 ? RedMarinePrefab : BlueMarinePrefab;
    }

    public static void GrantConsoleAccess(AccessConsole3D consoleAccessed)
    {
        SetAlarmActive(true);

        foreach (var accessConsole in instance.AccessConsoles.Where(x => x != consoleAccessed).ToList())
        {
            accessConsole.RequestConsoleLock(true, false);
        }
    }

    public static void RevokeConsoleAccess(AccessConsole3D consoleAccessed)
    {
        SetAlarmActive(false);

        foreach (var accessConsole in instance.AccessConsoles.Where(x => x != consoleAccessed).ToList())
        {
            accessConsole.RequestConsoleLock(false, false);
        }
    }

    public static void SetAlarmActive(bool active)
    {
        if (ShipManager.instance == null)
        {
            return;
        }

        ShipManager.instance.ApplyAlarmState(active);
        if (!PhotonNetwork.inRoom || PhotonNetwork.room == null)
        {
            return;
        }

        var props = new Hashtable();
        props[ShipManager.AlarmStateKey] = active;
        PhotonNetwork.room.SetCustomProperties(props);
    }

    public static TeleportController PickOfflineSpawn()
    {
        if (instance == null || instance.SpawnLocations == null)
        {
            return null;
        }

        var teamId = GetTeamId();
        var availableSpawnLocs = instance.SpawnLocations.Where(x => x != null && x.IsAvailable() && (teamId == 0 || x.TeamId == teamId)).ToArray();
        if (availableSpawnLocs.Length == 0)
        {
            return null;
        }

        return availableSpawnLocs[UnityEngine.Random.Range(0, availableSpawnLocs.Length)];
    }

    public static void ConsoleMsg(string msg, bool broadcast = true)
    {
        Debug.Log(msg);
        if (instance == null || instance.ConsolePanel == null)
        {
            return;
        }

        if (instance.ConsoleMessageCount + 1 > instance.ConsoleMessageBacklog)
        {
            instance.ClearConsole();
            instance.ConsoleMessageCount = 0;
        }
        else
        {
            instance.ConsoleMessageCount += 1;
        }

        instance.AppendConsoleMessage(msg);
    }

    public void Awake()
    {
        if (PrevGameCompletionTime as IntVariable == null)
        {
            Debug.LogError("PrevGameCompletionTime property is not IntVariable compatible");
        }
    }

    public void Start()
    {
        // Make sure the cursor is confined to the screen.
        Cursor.lockState = CursorLockMode.Confined;

        var devPlane = GameObject.Find("DevelopmentPlane");
        if (devPlane != null)
        {
            devPlane.SetActive(false);
        }

        ApplyEndedMatchFromRoom();

        /*if (PlayerManager3D.Get() != null)
        {
            if (PlayerManager3D.Get().GetLocalPlayer() == null)
            {
                var spawnLoc = GetTeleportForCurrentPlayer();
                if (spawnLoc == null)
                {
                    LeaveRoom();
                    return;
                }

                PlayerManager3D.Get().SpawnPlayer();
            }
        }*/
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            var animator = ConsolePanel.GetComponent<Animator>();
            if (animator != null)
            {
                // Toggle the console.
                animator.SetBool("IsShowing", !animator.GetBool("IsShowing"));
                IsConsoleShowing = animator.GetBool("IsShowing");
            }

            if (IsConsoleShowing)
            {
                var consoleCommand = ConsolePanel.transform.GetComponentInChildren<InputField>();
                if (consoleCommand != null)
                {
                    consoleCommand.ActivateInputField();
                }
            }
        }

        if (ReturnToLobbyBtn != null && PhotonNetwork.player != null)
        {
            ReturnToLobbyBtn.gameObject.SetActive(PhotonNetwork.player.IsMasterClient);
        }

        if (GameEndConditions.Any() && GameEndConditions.All(x => x))
        {
            RequestMatchEnd(2);

            // We have registered the game end, so reset all the game ending conditions
            // We cant remove them or the WinStatSystem will replace them.
            for(var i = 0; i < GameEndConditions.Count(); i++)
            {
                GameEndConditions[i] = false;
            }
        }
    }

    public override void OnLeftRoom()
    {
        ConsoleMsg("Game Manager: OnLeftRoom");
        SceneManager.LoadScene(0);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        ConsoleMsg(string.Format("OnPhotonPlayerConnected: Player Connected '{0}'", newPlayer.NickName));

        if (PhotonNetwork.isMasterClient)
        {
            ConsoleMsg(string.Format("OnPhotonPlayerConnected: IsMasterClient: {0}", PhotonNetwork.isMasterClient));
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        ConsoleMsg(string.Format("OnPhotonPlayerDisconnected: Player Disconnected '{0}'", otherPlayer.NickName));

        if (PhotonNetwork.isMasterClient)
        {
            ConsoleMsg(string.Format("OnPhotonPlayerDisconnected: IsMasterClient: {0}", PhotonNetwork.isMasterClient));
        }
    }

    public void LeaveRoom()
    {
        HaltGame();
        PhotonNetwork.LeaveRoom();
    }

    public void SetConsoleAccess(AccessConsole3D consoleAccessRequest)
    {
        if (!this.AccessConsoles.Any())
        {
            Debug.Log("No access consoles registed with the game manager.");
        }
    }

    public void ReturnToLobby()
    {
        ConsoleMsg("Game Manager: ReturnToLobby");
        if (!PhotonNetwork.isMasterClient)
        {
            ConsoleMsg("ERR: Trying to load level but we are not the master client.");
            return;
        }

        // Destroy all players.
        HaltGame();
        PhotonNetwork.LoadLevel(1);
    }

    /*Console Commands: Start*/
    public void ToggleDebug()
    {
        IsDebug = !IsDebug;
        ConsoleMsg(string.Format("Setting debug to {0}", IsDebug));
    }

    public void ClearConsole()
    {
        ConsolePanel.Find("MessageStream").GetComponentInChildren<Text>().text = string.Empty;
    }

    public void ToggleCamClamp()
    {
        if (PhotonNetwork.inRoom && instance.photonView != null)
        {
            instance.photonView.RPC("ToggleCameraClamp", PhotonNetworkSettings.EventTarget);
        }
        else
        {
            ToggleCameraClamp();
        }
    }

    public void ClearSteamAchievement(string id)
    {
        var steamAchievementManager = transform.root.Find("SteamAchievements");
        var steamAchievementController = steamAchievementManager.GetComponent<SteamAchievements>();
        if (steamAchievementController != null)
        {
            steamAchievementController.LockSteamAchievement(id);
        }
    }
    /*Console Commands: End*/

    public void RequestMatchEnd(int winningTeam)
    {
        if (matchEnded || IsMatchPhaseEnded())
        {
            return;
        }

        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        matchEnded = true;
        ShowWinPanel(winningTeam);

        if (!PhotonNetwork.inRoom || PhotonNetwork.room == null)
        {
            return;
        }

        var props = new Hashtable();
        props[MatchPhaseKey] = MatchPhaseEnded;
        props[WinningTeamKey] = winningTeam;
        PhotonNetwork.room.SetCustomProperties(props);

        if (photonView != null)
        {
            photonView.RPC("EndGame", PhotonNetworkSettings.EventTarget, winningTeam);
        }
    }

    public void AskMasterForTeam()
    {
        if (teamAssignmentRequested || GetTeamId() != 0)
        {
            return;
        }

        teamAssignmentRequested = true;
        if (!PhotonNetwork.inRoom || PhotonNetwork.player == null)
        {
            return;
        }

        if (PhotonNetwork.isMasterClient)
        {
            LobbyManager.AssignTeamId(PhotonNetwork.player);
            if (GetTeamId() != 0)
            {
                teamAssignmentRequested = false;
                var playerManager = FindObjectOfType<PlayerManager3D>();
                if (playerManager != null)
                {
                    playerManager.OnTeamAssigned();
                }
            }
            return;
        }

        if (photonView != null)
        {
            photonView.RPC("RequestTeamAssignment", PhotonTargets.MasterClient);
        }
    }

    [PunRPC]
    public void EndGame(int winningTeam)
    {
        if (IsMatchPhaseEnded() && winPanelShown)
        {
            return;
        }

        var team = winningTeam;
        if (IsMatchPhaseEnded())
        {
            team = ReadWinningTeam(winningTeam);
            ShowWinPanel(team);
            matchEnded = true;
            return;
        }

        ShowWinPanel(team);
        matchEnded = true;

        if (!PhotonNetwork.inRoom || !PhotonNetwork.isMasterClient || PhotonNetwork.room == null)
        {
            return;
        }

        var props = new Hashtable();
        props[MatchPhaseKey] = MatchPhaseEnded;
        props[WinningTeamKey] = team;
        PhotonNetwork.room.SetCustomProperties(props);
    }

    [PunRPC]
    public void RequestTeamAssignment(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || info.sender == null)
        {
            return;
        }

        LobbyManager.AssignTeamId(info.sender);
    }

    [PunRPC]
    public void RequestSpawnPoint(int teamId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || info.sender == null || photonView == null)
        {
            return;
        }

        if (SpawnLocations == null)
        {
            photonView.RPC("SpawnPointDenied", info.sender);
            return;
        }

        var available = SpawnLocations.Where(x => x != null && x.IsAvailable() && x.TeamId == teamId).ToArray();
        if (available.Length == 0)
        {
            photonView.RPC("SpawnPointDenied", info.sender);
            return;
        }

        var spawnLoc = available[UnityEngine.Random.Range(0, available.Length)];
        var index = Array.IndexOf(SpawnLocations, spawnLoc);
        spawnLoc.Spawned(null);
        photonView.RPC("SpawnPointGranted", info.sender, index);
    }

    [PunRPC]
    public void SpawnPointGranted(int spawnLocIndex)
    {
        var playerManager = FindObjectOfType<PlayerManager3D>();
        if (playerManager != null)
        {
            playerManager.CompleteSpawn(spawnLocIndex);
        }
    }

    [PunRPC]
    public void SpawnPointDenied()
    {
        var playerManager = FindObjectOfType<PlayerManager3D>();
        if (playerManager != null)
        {
            playerManager.FailSpawn();
        }
    }

    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        if (playerAndUpdatedProps == null || playerAndUpdatedProps.Length == 0)
        {
            return;
        }

        var player = playerAndUpdatedProps[0] as PhotonPlayer;
        if (player == null || !player.IsLocal || player.CustomProperties == null)
        {
            return;
        }

        if (!player.CustomProperties.ContainsKey(PlayerManager3D.PlayerTeamPrefKey))
        {
            return;
        }

        teamAssignmentRequested = false;
        var playerManager = FindObjectOfType<PlayerManager3D>();
        if (playerManager != null)
        {
            playerManager.OnTeamAssigned();
        }
    }

    public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged != null && propertiesThatChanged.ContainsKey(MatchPhaseKey))
        {
            ApplyEndedMatchFromRoom();
        }
    }

    public static int GetTeamId()
    {
        if (instance != null && instance.PlayerTeamOverride > 0)
        {
            return instance.PlayerTeamOverride;
        }

        if (!PhotonNetwork.inRoom || PhotonNetwork.player == null || PhotonNetwork.player.CustomProperties == null)
        {
            return 0;
        }

        if (!PhotonNetwork.player.CustomProperties.ContainsKey(PlayerManager3D.PlayerTeamPrefKey))
        {
            return 0;
        }

        return (int)PhotonNetwork.player.CustomProperties[PlayerManager3D.PlayerTeamPrefKey];
    }

    protected void HaltGame()
    {
        if (ShipManager.instance == null)
        {
            return;
        }

        var dontDestroyOnLoadObjects = ShipManager.instance.gameObject.scene.GetRootGameObjects();
        var destroyed = new HashSet<int>();
        foreach (var dontDestroyOnLoadObject in dontDestroyOnLoadObjects)
        {
            if (dontDestroyOnLoadObject == null)
            {
                continue;
            }

            var views = dontDestroyOnLoadObject.GetComponentsInChildren<PhotonView>(true);
            foreach (var view in views)
            {
                if (view == null || view.gameObject == null || !view.isMine)
                {
                    continue;
                }

                if (!destroyed.Add(view.gameObject.GetInstanceID()))
                {
                    continue;
                }

                if (PhotonNetwork.inRoom)
                {
                    if (view.isSceneView)
                    {
                        continue;
                    }

                    ConsoleMsg(string.Format("Destroy owned view {0}.", view.name));
                    PhotonNetwork.Destroy(view);
                }
                else
                {
                    ConsoleMsg(string.Format("Destroy owned view {0}.", view.name));
                    Destroy(view.gameObject);
                }
            }
        }
    }

    void AppendConsoleMessage(string msg)
    {
        var messageStream = ConsolePanel.Find("MessageStream");
        if (messageStream == null)
        {
            return;
        }

        var text = messageStream.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text += string.Format("\n{0}", msg);
        }
    }

    void ShowWinPanel(int winningTeam)
    {
        winPanelShown = true;
        if (GameStatePanel != null)
        {
            var winText = GameStatePanel.GetComponent<Text>();
            if (winText == null)
            {
                winText = GameStatePanel.GetComponentInChildren<Text>();
            }

            if (winText != null)
            {
                winText.text = SpaceRaceCopy.WinAnnouncement(winningTeam);
                winText.color = winningTeam == 1 ? SpaceRaceTheme.RedTeam : winningTeam == 2 ? SpaceRaceTheme.BlueTeam : SpaceRaceTheme.Text;
                winText.enabled = true;
            }
        }

        EnsureMatchEndChoices();

        if ((PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom) && PrevGameCompletionTime != null)
        {
            ((IntVariable)PrevGameCompletionTime).Value = (int)Time.timeSinceLevelLoad;
        }
    }

    void ApplyEndedMatchFromRoom()
    {
        if (!IsMatchPhaseEnded() || winPanelShown)
        {
            return;
        }

        ShowWinPanel(ReadWinningTeam(0));
        matchEnded = true;
    }

    static bool IsMatchPhaseEnded()
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.room == null || PhotonNetwork.room.CustomProperties == null)
        {
            return false;
        }

        return PhotonNetwork.room.CustomProperties.ContainsKey(MatchPhaseKey) &&
               (string)PhotonNetwork.room.CustomProperties[MatchPhaseKey] == MatchPhaseEnded;
    }

    static int ReadWinningTeam(int fallback)
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.room == null || PhotonNetwork.room.CustomProperties == null)
        {
            return fallback;
        }

        if (!PhotonNetwork.room.CustomProperties.ContainsKey(WinningTeamKey))
        {
            return fallback;
        }

        return (int)PhotonNetwork.room.CustomProperties[WinningTeamKey];
    }

    Button rematchButton;

    void EnsureMatchEndChoices()
    {
        var hostCanChoose = !PhotonNetwork.inRoom || PhotonNetwork.isMasterClient;
        if (ReturnToLobbyBtn != null)
        {
            var label = ReturnToLobbyBtn.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = "Lobby";
            }

            ReturnToLobbyBtn.gameObject.SetActive(hostCanChoose);
        }

        if (rematchButton == null && ReturnToLobbyBtn != null && ReturnToLobbyBtn.transform.parent != null)
        {
            rematchButton = SpaceRaceWidgets.CreateButton(ReturnToLobbyBtn.transform.parent, "Rematch", "Rematch", Rematch);
            var source = ReturnToLobbyBtn.GetComponent<RectTransform>();
            var rect = rematchButton.GetComponent<RectTransform>();
            if (source != null && rect != null)
            {
                rect.anchorMin = source.anchorMin;
                rect.anchorMax = source.anchorMax;
                rect.pivot = source.pivot;
                rect.sizeDelta = source.sizeDelta;
                rect.anchoredPosition = source.anchoredPosition + new Vector2(0f, source.sizeDelta.y + 8f);
            }
        }

        if (rematchButton != null)
        {
            rematchButton.gameObject.SetActive(hostCanChoose);
        }
    }

    public void Rematch()
    {
        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        if (PhotonNetwork.inRoom && PhotonNetwork.room != null)
        {
            var props = new Hashtable();
            props[MatchPhaseKey] = "Playing";
            props[WinningTeamKey] = 0;
            PhotonNetwork.room.SetCustomProperties(props);
        }

        matchEnded = false;
        winPanelShown = false;
        var scene = SceneManager.GetActiveScene().name;
        if (PhotonNetwork.inRoom)
        {
            PhotonNetwork.LoadLevel(scene);
        }
        else
        {
            SceneManager.LoadScene(scene);
        }
    }

    void ScheduleReturnToLobby()
    {
        if (lobbyReturnScheduled)
        {
            return;
        }

        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        lobbyReturnScheduled = true;
        StartCoroutine(TimedReturnToLobby());
    }

    [PunRPC]
    protected void ToggleCameraClamp()
    {
        var mainCamera = Camera.main.GetComponent<CameraFollow3D>();
        if (mainCamera != null)
        {
            var tmpAngle = mainCamera.Y_ANGLE_MIN == 85f ? 45f : 85f;
            mainCamera.Y_ANGLE_MIN = tmpAngle;
            ConsoleMsg(string.Format("CamClamp min set to {0}", tmpAngle));
        }
    }

    protected IEnumerator TimedReturnToLobby()
    {
        yield return new WaitForSeconds(10);
        ReturnToLobby();
    }
}
