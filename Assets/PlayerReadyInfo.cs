using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReadyInfo : Photon.MonoBehaviour
{
    public const string PlayerReadyKey = "PlayerReady";

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
            ApplyReadyState(IsPlayerReady(Player));

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
        if (Player == null)
        {
            return;
        }

        var readyProps = new Hashtable();
        readyProps[PlayerReadyKey] = !IsPlayerReady(Player);
        Player.SetCustomProperties(readyProps);
    }

    public static bool IsPlayerReady(PhotonPlayer player)
    {
        if (player == null || player.CustomProperties == null || !player.CustomProperties.ContainsKey(PlayerReadyKey))
        {
            return false;
        }

        return (bool)player.CustomProperties[PlayerReadyKey];
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

    public void ApplyReadyState(bool state)
    {
        PlayerReady = state;

        PlayerReadyButton.GetComponent<Image>().sprite = PlayerReady
            ? ReadyStateSprite
            : UnreadyStateSprite;
    }
}
