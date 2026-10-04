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

    public enum SceneToLoadEnum
    {
        ShipBoardingPvp,
        SpaceStationPve,
        BioWarsTest
    }

    public SceneToLoadEnum SceneToLoad;

    public void Start()
    {
        PhotonNetwork.room.IsOpen = true;
        PlayersInLobby = new List<PhotonPlayer>();
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;

        if (PhotonNetwork.isMasterClient)
        {
            AssignTeamId(PhotonNetwork.player);
            this.photonView.RPC("UpdatePlayerInfo", PhotonNetworkSettings.DefaultRPCNetworkTarget);
        }
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
        Debug.Log(string.Format("{0} joined the room for team {1}", newPlayer.NickName, newPlayer.CustomProperties[PlayerManager3D.PlayerTeamPrefKey]));
        if (PhotonNetwork.isMasterClient)
        {
            AssignTeamId(newPlayer);
            this.photonView.RPC("UpdatePlayerInfo", PhotonNetworkSettings.DefaultRPCNetworkTarget);
        }
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
        Debug.Log(string.Format("{0} left the room from team {1}", otherPlayer.NickName, otherPlayer.CustomProperties[PlayerManager3D.PlayerTeamPrefKey]));
        if (PhotonNetwork.isMasterClient)
        {
            this.photonView.RPC("UpdatePlayerInfo", PhotonNetworkSettings.DefaultRPCNetworkTarget);
        }
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

            // Clear the lobby and reload all players.
            photonView.RPC("UpdatePlayerInfo", PhotonTargets.AllViaServer);
        }
    }

    public void SwitchTeam()
    {
        // Determine which team the player should be on by default.
        var currentTeamId = (int)PhotonNetwork.player.CustomProperties[PlayerManager3D.PlayerTeamPrefKey];

        // Switch the team id.
        var teamId = currentTeamId == 1 ? 2 : 1;

        // Make sure we've not somehow switched to the team we were already on.
        if (teamId != currentTeamId)
        {
            // Assign the team id
            AssignTeamId(PhotonNetwork.player, teamId);

            // Update the lobby for everyone.
            photonView.RPC("UpdatePlayerInfo", PhotonNetworkSettings.DefaultRPCNetworkTarget);
        }
    }

    public void RotateTeams()
    {
        var teamOnePlayers = PhotonNetwork.playerList.Where(x => (int)x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] == 1).ToList();
        var teamTwoPlayers = PhotonNetwork.playerList.Where(x => (int)x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] == 2).ToList();

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

        // Update the lobby for everyone.
        photonView.RPC("UpdatePlayerInfo", PhotonNetworkSettings.DefaultRPCNetworkTarget);
    }

    protected void AssignTeamId(PhotonPlayer player, int? teamId = null)
    {
        // If no team ID has been specified.
        if (!teamId.HasValue)
        {
            // Determine which team the player should be on by default.
            var playerListExCurrent = PhotonNetwork.playerList.Where(x => x.ID != player.ID).ToList();
            var redTeamCount =
                playerListExCurrent.Count(
                    x =>
                        x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] != null &&
                        ((int) x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey]) == 1);
            var blueTeamCount =
                playerListExCurrent.Count(
                    x =>
                        x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] != null &&
                        ((int) x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey]) == 2);

            // Set the player prefs.
            teamId = redTeamCount < blueTeamCount ? 1 : 2;
        }

        // Get the custom properties from th player.
        var localPlayerCustomProps = player.CustomProperties;

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
            playerInfo.transform.SetParent((int)newPlayer.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] == 1 ? RedTeam.transform : BlueTeam.transform);
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
        var teamPanel = (int)newPlayer.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] == 1 ? RedTeam.transform : BlueTeam.transform;

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

        // Make sure the game object is active.
        LaunchButton.gameObject.SetActive(PhotonNetwork.player.IsMasterClient);

        // And the button is enabled.
        LaunchButton.enabled = PhotonNetwork.player.IsMasterClient;
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
            PhotonNetwork.automaticallySyncScene = true;
            PhotonNetwork.LoadLevel(SceneToLoad.ToString());
        }
    }
 }
