using Assets.Scripts.Interfaces;
using UnityEngine;

public class PunNetworkManager : Photon.PunBehaviour, IPunNetworkManager
{
    public bool OfflineMode;

    public bool LocalSceneAutoJoin;

    public PhotonLogLevel LogLevel = PhotonLogLevel.ErrorsOnly;

    public byte MaxPlayersPerRoom = 6;

    protected bool IsConnecting;

    public bool SteamActive;

    protected virtual void Awake()
    {
        PhotonNetwork.autoJoinLobby = false;
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.logLevel = LogLevel;

        if (LocalSceneAutoJoin)
        {
            Connect();
        }
    }

    public virtual void Connect()
    {
        Debug.Log("PunNetworkManagerTest.Connect");
        IsConnecting = true;

        if (PhotonNetwork.connected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.offlineMode = OfflineMode;
            PhotonNetwork.ConnectUsingSettings("SpaceRace");
        }
    }

    public virtual void QuitGame()
    {
        Application.Quit();
    }

    public override void OnConnectedToMaster()
    {
        if (IsConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public virtual void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = MaxPlayersPerRoom }, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        var playerManager = FindObjectOfType<PlayerManager3D>();
        if (playerManager != null)
        {
            playerManager.SpawnPlayer();
        }
    }
}
