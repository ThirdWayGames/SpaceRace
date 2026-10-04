using Assets.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

public class GameStateManager : Photon.PunBehaviour
{
    public HealthComponent RedCoreHealthComponent;

    public HealthComponent BlueCoreHealthComponent;

    public int LobbyReturnTimeout = 10;

    public Text GameStateWinTimeText;

    public void Awake()
    {
        if (RedCoreHealthComponent == null)
        {
            Debug.LogErrorFormat("RedCoreHealthComponent property is not set for '{0}'", this.name);
        }

        if (BlueCoreHealthComponent == null)
        {
            Debug.LogErrorFormat("BlueCoreHealthComponent property is not set for '{0}'", this.name);
        }

        if (GameStateWinTimeText == null)
        {
            Debug.LogErrorFormat("GameStateWinTimeText property is not set for '{0}'", this.name);
        }
    }

    public void Update()
    {
        var gameOver = false;
        var winningTeam = string.Empty;

        // Determine if the game is over and who has won.
        if (RedCoreHealthComponent.CurrentValue <= 0 || BlueCoreHealthComponent.CurrentValue <= 0)
        {
            winningTeam = RedCoreHealthComponent.CurrentValue > 0 ? "RED" : "BLUE";
            gameOver = true;
        } 

        // If we have the game win text
        if (GameStateWinTimeText != null)
        {
            // Set the values
            GameStateWinTimeText.text = string.Format("{0} TEAM WINS", winningTeam);
            GameStateWinTimeText.enabled = gameOver;
        }

        // if you are the master client and the lobbyManger is null (which means we are in a game and not the lobby)
        var lobbyManager = FindObjectOfType<LobbyManager>();
        if (PhotonNetwork.isMasterClient && lobbyManager == null)
        {
            // Determine if the game is over.
            if (gameOver)
            {
                // If we are in a network room
                if (PhotonNetwork.inRoom)
                {
                    // Network end the game.
                    GameManager3D.instance.GetComponent<PhotonView>().RPC("EndGame", PhotonNetworkSettings.DefaultRPCNetworkTarget, 1);
                }
                else
                {
                    // Local end the game.
                    GameManager3D.instance.EndGame(1);
                }
            }
        }
    }
}
