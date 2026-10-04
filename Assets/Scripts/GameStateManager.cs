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
        var winningTeamId = 0;

        // Determine if the game is over and who has won.
        // Team 1 is red, team 2 is blue. The core that is still alive wins.
        if (RedCoreHealthComponent.CurrentValue <= 0 || BlueCoreHealthComponent.CurrentValue <= 0)
        {
            winningTeamId = RedCoreHealthComponent.CurrentValue > 0 ? 1 : 2;
            winningTeam = winningTeamId == 1 ? "RED" : "BLUE";
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
        if (PhotonNetwork.isMasterClient && lobbyManager == null && gameOver && GameManager3D.instance != null)
        {
            GameManager3D.instance.RequestMatchEnd(winningTeamId);
        }
    }
}
