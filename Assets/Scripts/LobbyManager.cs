using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : Photon.PunBehaviour
{
    public GameObject PlayerInfoPrefab;
    public RectTransform BlueTeam;
    public RectTransform RedTeam;
    public List<PhotonPlayer> PlayersInLobby;
    public Button LaunchButton;
    public Button SwitchTeamsButton;
    public Dropdown ArenaName;

    public Canvas LobbyCanvas;
    public Canvas LoadingScreen;

    Text waitingText;

    GameObject howToPanel;

    bool howToBuilt;

    public enum SceneToLoadEnum
    {
        ShipBoardingPvp,
        SpaceStationPve,
        BioWarsTest
    }

    public SceneToLoadEnum SceneToLoad;

    public void Start()
    {
        SetupLobbyPresentation();

        if (PhotonNetwork.room == null)
        {
            return;
        }

        PhotonNetwork.room.IsOpen = true;
        PlayersInLobby = new List<PhotonPlayer>();

        if (PhotonNetwork.isMasterClient)
        {
            AssignTeamId(PhotonNetwork.player);
        }

        UpdatePlayerInfo();
    }

    /// <summary>
    /// Called when the local user/client left a room.
    /// </summary>
    /// <remarks>
    /// When leaving a room, PUN brings you back to the Master Server.
    /// Before you can use lobbies and join or create rooms, OnJoinedLobby() or OnConnectedToMaster() will get called again.
    /// </remarks>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Called when a remote player entered the room. This PhotonPlayer is already added to the playerlist at this time.
    /// </summary>
    /// <param name="newPlayer"></param>
    /// <remarks>
    /// If your game starts with a certain number of players, this callback can be useful to check the
    /// Room.playerCount and find out if you can start.
    /// </remarks>
    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log(string.Format("{0} joined the room for team {1}", newPlayer.NickName, TeamLabel(newPlayer)));
        if (PhotonNetwork.isMasterClient)
        {
            AssignTeamId(newPlayer);
        }

        UpdatePlayerInfo();
    }

    /// <summary>
    /// Called when a remote player left the room. This PhotonPlayer is already removed from the playerlist at this time.
    /// </summary>
    /// <param name="otherPlayer"></param>
    /// <remarks>
    /// When your client calls PhotonNetwork.leaveRoom, PUN will call this method on the remaining clients.
    /// When a remote client drops connection or gets closed, this callback gets executed. after a timeout
    /// of several seconds.
    /// </remarks>
    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        Debug.Log(string.Format("{0} left the room from team {1}", otherPlayer.NickName, TeamLabel(otherPlayer)));
        UpdatePlayerInfo();
    }

    /// <summary>
    /// Leaves the room.
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Loads the arena.
    /// </summary>
    public void LoadArena()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            Debug.Log("ERR: Trying to load level but we are not the master client.");
        }

        if (PhotonNetwork.isMasterClient)
        {
            if (PhotonNetwork.inRoom)
            {
                photonView.RPC("ShowLoadingScreen", PhotonTargets.AllViaServer);
            }
            else
            {
                ShowLoadingScreen();
            }
        }
    }

    public void MakeMasterClient(PhotonPlayer player)
    {
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.SetMasterClient(player);
            UpdatePlayerInfo();
        }
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        UpdatePlayerInfo();
    }

    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        UpdatePlayerInfo();
    }

    public void SwitchTeam()
    {
        // Determine which team the player should be on by default.
        if (PhotonNetwork.player == null || PhotonNetwork.player.CustomProperties == null || !PhotonNetwork.player.CustomProperties.ContainsKey(PlayerManager3D.PlayerTeamPrefKey))
        {
            return;
        }

        var currentTeamId = (int)PhotonNetwork.player.CustomProperties[PlayerManager3D.PlayerTeamPrefKey];

        // Switch the team id.
        var teamId = currentTeamId == 1 ? 2 : 1;

        // Make sure we've not somehow switched to the team we were already on.
        if (teamId != currentTeamId)
        {
            // Assign the team id
            AssignTeamId(PhotonNetwork.player, teamId);

            UpdatePlayerInfo();
        }
    }

    public void RotateTeams()
    {
        var teamOnePlayers = PhotonNetwork.playerList.Where(x => ReadTeamId(x) == 1).ToList();
        var teamTwoPlayers = PhotonNetwork.playerList.Where(x => ReadTeamId(x) == 2).ToList();

        // Switch the team one players
        foreach (var player in teamOnePlayers)
        {
            AssignTeamId(player, 2);
        }

        // Switch the team two players
        foreach (var player in teamTwoPlayers)
        {
            AssignTeamId(player, 1);
        }

        UpdatePlayerInfo();
    }

    public static void AssignTeamId(PhotonPlayer player, int? teamId = null)
    {
        if (player == null)
        {
            return;
        }

        // If no team ID has been specified.
        if (!teamId.HasValue)
        {
            if (player.CustomProperties != null && player.CustomProperties.ContainsKey(PlayerManager3D.PlayerTeamPrefKey))
            {
                return;
            }

            // Determine which team the player should be on by default.
            var playerListExCurrent = PhotonNetwork.playerList.Where(x => x.ID != player.ID).ToList();
            var redTeamCount = playerListExCurrent.Count(x => ReadTeamId(x) == 1);
            var blueTeamCount = playerListExCurrent.Count(x => ReadTeamId(x) == 2);

            // Set the player prefs.
            teamId = redTeamCount < blueTeamCount ? 1 : 2;
        }

        // Get the custom properties from th player.
        var localPlayerCustomProps = player.CustomProperties ?? new ExitGames.Client.Photon.Hashtable();

        // Update the local custom props
        localPlayerCustomProps[PlayerManager3D.PlayerTeamPrefKey] = teamId;

        // Update the player custom props.
        Debug.Log(string.Format("Setting player '{0}' to team {1}", player.NickName, teamId));
        player.SetCustomProperties(localPlayerCustomProps);
    }

    /// <summary>
    /// Adds the player.
    /// </summary>
    /// <param name="newPlayer">The new player.</param>
    protected void AddPlayer(PhotonPlayer newPlayer)
    {
        if (newPlayer != null && PhotonNetwork.inRoom)
        {
            var playerInfo = Instantiate(PlayerInfoPrefab, Vector3.zero, Quaternion.identity);
            var playerInfoScript = playerInfo.GetComponent<PlayerReadyInfo>();
            playerInfoScript.SetName(newPlayer.NickName);
            playerInfoScript.SetPlayer(newPlayer);
            playerInfo.transform.SetParent(ReadTeamId(newPlayer) == 1 ? RedTeam.transform : BlueTeam.transform);
        }
    }

    protected void RemoveTeam(int teamId)
    {
        var teamPanel = teamId == 1 ? RedTeam.transform : BlueTeam.transform;

        // Get the children, remove the parents.
        var playerInfoPanes = teamPanel.gameObject.GetComponentsInChildren<PlayerReadyInfo>().ToList();
        foreach (var playerInfo in playerInfoPanes)
        {
            Destroy(playerInfo.gameObject);
        }
    }

    protected void RemovePlayer(PhotonPlayer newPlayer)
    {
        var teamPanel = ReadTeamId(newPlayer) == 1 ? RedTeam.transform : BlueTeam.transform;

        // Get the children, remove the parents.
        var playerInfoPane = teamPanel.gameObject.GetComponentsInChildren<PlayerReadyInfo>().ToList();
        var playerInfo = playerInfoPane.FirstOrDefault(x => x.Player.ID == newPlayer.ID);

        if (playerInfo != null)
        {
            Destroy(playerInfo.gameObject);
        }
    }

    [PunRPC]
    public void UpdatePlayerInfo()
    {
        if (PhotonNetwork.room == null || !PhotonNetwork.inRoom)
        {
            return;
        }

        if (PlayersInLobby == null)
        {
            PlayersInLobby = new List<PhotonPlayer>();
        }

        // remove all players.
        RemoveTeam(1);
        RemoveTeam(2);
        PlayersInLobby.Clear();

        // Update the list of photon players in the "lobby"
        var photonPlayers = PhotonNetwork.playerList.ToList();
        foreach (var photonPlayer in photonPlayers)
        {
            if (!PlayersInLobby.Contains(photonPlayer))
            {
                PlayersInLobby.Add(photonPlayer);

                // Add the new player to the "lobby"
                AddPlayer(photonPlayer);
            }
        }

        foreach (var photonPlayer in PlayersInLobby)
        {
            if (!photonPlayers.Contains(photonPlayer))
            {
                RemovePlayer(photonPlayer);
            }
        }

        var isMaster = PhotonNetwork.player != null && PhotonNetwork.player.IsMasterClient;
        var playerCount = PhotonNetwork.playerList == null ? 0 : PhotonNetwork.playerList.Length;
        var crewReady = AllPlayersReady();
        var canLaunch = HostCanLaunch(isMaster, playerCount, crewReady);

        // Make sure the game object is active.
        LaunchButton.gameObject.SetActive(isMaster);

        // A solo host can launch immediately. A larger crew waits until everyone is ready.
        LaunchButton.enabled = isMaster;
        LaunchButton.interactable = canLaunch;

        if (waitingText != null)
        {
            waitingText.gameObject.SetActive(isMaster && playerCount > 1 && !crewReady);
        }
    }

    public static bool HostCanLaunch(bool isMaster, int playerCount, bool crewReady)
    {
        if (!isMaster)
        {
            return false;
        }

        return playerCount <= 1 || crewReady;
    }

    static bool AllPlayersReady()
    {
        if (PhotonNetwork.playerList == null || PhotonNetwork.playerList.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            if (!PlayerReadyInfo.IsPlayerReady(PhotonNetwork.playerList[i]))
            {
                return false;
            }
        }

        return true;
    }

    void SetupLobbyPresentation()
    {
        var keys = GameObject.Find("KeyBindings");
        if (keys != null)
        {
            keys.SetActive(false);
        }

        var canvasObject = LobbyCanvas != null ? LobbyCanvas.gameObject : GameObject.Find("LobbyCanvas");
        if (canvasObject == null)
        {
            return;
        }

        var canvas = canvasObject.transform;
        var howToTransform = canvas.Find("HowToPlay");
        var howTo = howToTransform != null
            ? howToTransform.GetComponent<Button>()
            : SpaceRaceWidgets.CreateButton(canvas, "HowToPlay", "How to play", ToggleHowToPlay);
        PlaceCornerButton(howTo);

        if (waitingText == null)
        {
            waitingText = SpaceRaceWidgets.CreateText(canvas, "WaitingForCrew", SpaceRaceCopy.WaitingForCrew, 16, SpaceRaceTheme.Orange, TextAnchor.MiddleCenter);
        }

        var waitRect = waitingText.rectTransform;
        waitRect.anchorMin = new Vector2(0.18f, 0.9f);
        waitRect.anchorMax = new Vector2(0.72f, 0.97f);
        waitRect.offsetMin = Vector2.zero;
        waitRect.offsetMax = Vector2.zero;
        waitingText.gameObject.SetActive(false);

        var objective = GameObject.Find("LocateText");
        if (objective != null)
        {
            var objectiveText = objective.GetComponent<Text>();
            if (objectiveText != null)
            {
                objectiveText.text = SpaceRaceCopy.WinCondition;
            }
        }
    }

    void ToggleHowToPlay()
    {
        if (!howToBuilt)
        {
            var canvasObject = LobbyCanvas != null ? LobbyCanvas.gameObject : GameObject.Find("LobbyCanvas");
            if (canvasObject == null)
            {
                return;
            }

            var panel = SpaceRaceWidgets.CreateHowToCard(canvasObject.transform, ToggleHowToPlay);
            panel.transform.SetAsLastSibling();
            howToPanel = panel;
            howToBuilt = true;
            return;
        }

        if (howToPanel != null)
        {
            howToPanel.SetActive(!howToPanel.activeSelf);
            if (howToPanel.activeSelf)
            {
                howToPanel.transform.SetAsLastSibling();
            }
        }
    }

    static void PlaceCornerButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        var rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-16f, -16f);
        rect.sizeDelta = new Vector2(168f, 36f);
        button.transform.SetAsLastSibling();
    }

    [PunRPC]
    public void ShowLoadingScreen()
    {
        if (LobbyCanvas != null && LoadingScreen != null)
        {
            LoadingScreen.gameObject.SetActive(true);
            LobbyCanvas.gameObject.SetActive(false);
        }

        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.room.IsOpen = false;
            PhotonNetwork.LoadLevel(GetSceneName(SceneToLoad));
        }
    }

    public static string GetSceneName(SceneToLoadEnum scene)
    {
        switch (scene)
        {
            case SceneToLoadEnum.ShipBoardingPvp:
                return "ShipBoardingPvp";
            case SceneToLoadEnum.SpaceStationPve:
                return "SpaceStationPve";
            case SceneToLoadEnum.BioWarsTest:
                return "BioWarsTest";
            default:
                return "SpaceStationPve";
        }
    }

    static int ReadTeamId(PhotonPlayer player)
    {
        if (player == null || player.CustomProperties == null || !player.CustomProperties.ContainsKey(PlayerManager3D.PlayerTeamPrefKey))
        {
            return 0;
        }

        return (int)player.CustomProperties[PlayerManager3D.PlayerTeamPrefKey];
    }

    static string TeamLabel(PhotonPlayer player)
    {
        var teamId = ReadTeamId(player);
        return teamId == 0 ? "unassigned" : teamId.ToString();
    }
 }
