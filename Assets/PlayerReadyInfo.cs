using System.Security;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReadyInfo : Photon.MonoBehaviour
{
    public PhotonPlayer Player;
    public Button PlayerReadyButton;
    public Sprite ReadyStateSprite;
    public Sprite UnreadyStateSprite;
    public Text PlayerNameText;
    public Image MasterClientImg;
    public GameObject MakeMasterClientButton;

    protected bool PlayerReady = false;
    
    public void Start()
    {
        PlayerReadyButton.enabled = true;
        if (Player != null)
        { 
            MasterClientImg.enabled = Player.IsMasterClient;

            SetName(Player.NickName);

            // If I am the master client and the other player is not.
            MakeMasterClientButton.SetActive(!Player.IsMasterClient && PhotonNetwork.isMasterClient);
        }
        else
        {
            Debug.LogWarning("PlayerReadyInfo.Start(): Player is not set");
        }
    }

    public void ToggleReadyState()
    {
        this.photonView.RPC("SendNetworkReadyState", PhotonNetworkSettings.DefaultRPCNetworkTarget, new object[] { !PlayerReady });        
    }

    /// <summary>
    /// Sets the player.
    /// </summary>
    /// <param name="player">The player.</param>
    public void SetPlayer(PhotonPlayer player)
    {
        Player = player;
    }

    public void SetName(string playerName)
    {
        PlayerNameText.text = playerName;
    }

    public void MakeMasterClient()
    {
        var lobbyManager = (LobbyManager)FindObjectOfType(typeof(LobbyManager));
        if (lobbyManager != null)
        {
            lobbyManager.MakeMasterClient(Player);
        }
    }

    [PunRPC]
    public void SendNetworkReadyState(bool state)
    {
        // Set the player ready state.
        PlayerReady = state;

        PlayerReadyButton.GetComponent<Image>().sprite = PlayerReady
            ? ReadyStateSprite
            : UnreadyStateSprite;
    }
}