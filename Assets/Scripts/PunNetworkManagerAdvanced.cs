using Assets.Scripts.Interfaces;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class PunNetworkManagerAdvanced : PunNetworkManager, IPunNetworkManager
{
    public Text Username;

    public InputField PlayerName;

    public Button QuickMatchButton;

    [Tooltip("The UI panel to let the user enter their name, connect and play.")]
    public GameObject ControlPanel;

    [Tooltip("The UI label to inform the user that the connection is in progress.")]
    public GameObject ProgressLabel;

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = false;
        PhotonNetwork.automaticallySyncScene = false;
        PhotonNetwork.logLevel = LogLevel;

        if (LocalSceneAutoJoin)
        {
            Connect();
        }
    }

    public void Start()
    {
        if (ProgressLabel != null)
        {
            ProgressLabel.SetActive(false);
        }

        if (ControlPanel != null)
        {
            ControlPanel.SetActive(true);
        }

        SteamActive = SteamManager.Initialized;

        if (Username != null)
        {
            Username.gameObject.SetActive(SteamActive);
        }

        if (PlayerName != null)
        {
            PlayerName.gameObject.SetActive(!SteamActive);
        }

        // Setup the default playername.
        if (SteamActive)
        {
            var defaultName = SteamFriends.GetPersonaName();
            PhotonNetwork.playerName = defaultName;

            if (Username != null)
            {
                Username.text = defaultName;
            }
        }
    }

    public void Update()
    {
        if (QuickMatchButton != null)
        {
            QuickMatchButton.interactable = SteamActive || (!string.IsNullOrEmpty(PlayerName.textComponent.text) && PlayerName.textComponent.text.Length > 3);
        }
    }

    public override void Connect()
    {
        if (ProgressLabel != null)
        {
            ProgressLabel.SetActive(true);
        }

        if (ControlPanel != null)
        {
            ControlPanel.SetActive(false);
        }

        base.Connect();
    }

    public void OnGUI()
    {
        if (ProgressLabel != null)
        {
            ProgressLabel.GetComponent<Text>().text = PhotonNetwork.connectionStateDetailed.ToString();
        }
    }

    public override void OnDisconnectedFromPhoton()
    {
        if (ProgressLabel != null)
        {
            ProgressLabel.SetActive(false);
        }

        if (ControlPanel != null)
        {
            ControlPanel.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(1);
    }
}
