using Assets.Scripts.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private bool IsConsoleShowing = false;

    private int ConsoleMessageBacklog = 30;

    private int ConsoleMessageCount = 0;

    public GameObject GetMarinePrefab()
    {
        return GetTeamId() == 1 ? RedMarinePrefab : BlueMarinePrefab;
    }

    public static void GrantConsoleAccess(AccessConsole3D consoleAccessed)
    {
        // Get the ship manager to raise the alarm.
        var shipManager = ShipManager.instance;
        if (shipManager != null)
        {
            if (PhotonNetwork.inRoom)
            {
                ShipManager.instance.photonView.RPC("RaiseAlarm", PhotonNetworkSettings.DefaultRPCNetworkTarget);
            }
            else
            {
                ShipManager.instance.RaiseAlarm();
            }
        }

        // Lock all other consoles.
        foreach (var accessConsole in instance.AccessConsoles.Where(x => x != consoleAccessed).ToList())
        {
            if (PhotonNetwork.inRoom)
            {
                accessConsole.photonView.RPC("LockConsole", PhotonNetworkSettings.DefaultRPCNetworkTarget, false);
            }
            else
            {
                accessConsole.LockConsole(false);
            }
        }
    }

    public static void RevokeConsoleAccess(AccessConsole3D consoleAccessed)
    {
        var shipManager = ShipManager.instance;
        if (shipManager != null)
        {
            if (PhotonNetwork.inRoom)
            {
                ShipManager.instance.photonView.RPC("CancelAlarm", PhotonNetworkSettings.DefaultRPCNetworkTarget);
            }
            else
            {
                ShipManager.instance.CancelAlarm();
            }
        }

        // Unlock all other consoles.
        foreach (var accessConsole in instance.AccessConsoles.Where(x => x != consoleAccessed).ToList())
        {
            if (PhotonNetwork.inRoom)
            {
                accessConsole.photonView.RPC("UnlockConsole", PhotonNetworkSettings.DefaultRPCNetworkTarget);
            }
            else
            {
                accessConsole.UnlockConsole();
            }
        }
    }

    public static TeleportController GetTeleportForCurrentPlayer()
    {
        TeleportController spawnLoc = null;
        ConsoleMsg("GetTeleportForCurrentPlayer");
        try
        {
            var teamId = GetTeamId();
            teamId = teamId == 0 ? UnityEngine.Random.Range(1, 2) : teamId;
            ConsoleMsg(string.Format("Getting Spawn Location for team '{0}'", teamId));

            var availableSpawnLocs = instance.SpawnLocations.Where(x => x.IsAvailable() && x.TeamId == teamId).ToArray();
            spawnLoc = availableSpawnLocs[UnityEngine.Random.Range(0, availableSpawnLocs.Count())];
            if (spawnLoc == null)
            {
                ConsoleMsg(string.Format("No spawn locations found for player '{0}'", teamId));
            }

            ConsoleMsg(string.Format("Spawn Loc found for team '{0}'", teamId));
            var spawnLocIndex = instance.SpawnLocations.ToList().IndexOf(spawnLoc);
            if (PhotonNetwork.inRoom && PhotonNetwork.player != null && instance.photonView != null)
            {
                instance.photonView.RPC("NetworkFlagSpawnInUse", PhotonNetworkSettings.DefaultRPCNetworkTarget, new object[] { spawnLocIndex });
            }
            else
            {
                instance.NetworkFlagSpawnInUse(spawnLocIndex);
            }
        }
        catch (Exception ex)
        {
            ConsoleMsg(ex.Message);
        }

        return spawnLoc;

    }

    public static void ConsoleMsg(string msg, bool broadcast = true)
    {
        if (instance != null)
        {
            if (instance.ConsoleMessageCount + 1 > instance.ConsoleMessageBacklog)
            {
                instance.ClearConsole();
                instance.ConsoleMessageCount = 0;
            }
            else
            {
                instance.ConsoleMessageCount += 1;
            }

            // Add the message to the console output.
            if (instance.photonView != null && PhotonNetwork.inRoom && broadcast)
            {
                instance.photonView.RPC("NetworkConsoleMsg", PhotonNetworkSettings.DefaultRPCNetworkTarget, new object[] { string.Format("'{0}': {1}", PhotonNetwork.player != null ? PhotonNetwork.player.NickName : "No Player", msg) });
            }
            else
            {
                instance.NetworkConsoleMsg(msg);
            }
        }
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

        if (ReturnToLobbyBtn != null)
        {
            ReturnToLobbyBtn.gameObject.SetActive(PhotonNetwork.player.IsMasterClient);
        }

        if (GameEndConditions.Any())
        {
            if (GameEndConditions.All(x => x))
            {
                // End the game.
                if (PhotonNetwork.inRoom && this.photonView != null)
                {
                    this.photonView.RPC("EndGame", PhotonNetworkSettings.DefaultRPCNetworkTarget, 2);
                }
                else
                {
                    EndGame(0);
                }

                // We have registered the game end, so reset all the game ending conditions
                // We cant remove them or the WinStatSystem will replace them.
                for(var i = 0; i < GameEndConditions.Count(); i++)
                {
                    GameEndConditions[i] = false;
                }
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
        instance.photonView.RPC("ToggleCameraClamp", PhotonNetworkSettings.DefaultRPCNetworkTarget);
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

    [PunRPC]
    public void EndGame(int winningTeam)
    {
        var winText = instance.GameStatePanel.GetComponent<Text>();
        if (winText != null)
        {
            ////winText.text = string.Format("{0} TEAM WINS!", winningTeam == 2 ? "RED" : "BLUE");
            winText.enabled = true;
        }

        if (PhotonNetwork.isMasterClient)
        {
            if (PrevGameCompletionTime != null)
            {
                // Store the time it took to win the game.
                ((IntVariable)PrevGameCompletionTime).Value = (int)Time.timeSinceLevelLoad;
            }
        }

        StartCoroutine("TimedReturnToLobby");
    }

    protected static int GetTeamId()
    {
        if (instance.PlayerTeamOverride > 0)
        {
            return instance.PlayerTeamOverride;
        }

        var teamId = 0;
        if (PhotonNetwork.player.CustomProperties != null && PhotonNetwork.player.CustomProperties.Count > 0)
        {
            teamId = (int)PhotonNetwork.player.CustomProperties[PlayerManager3D.PlayerTeamPrefKey];
        }
        else
        {
            // Assign to the team with the lowest number of players and set the player team.
            var redTeamCount = PhotonNetwork.playerList.Where(x => x.ID != PhotonNetwork.player.ID).Count(x => PhotonNetwork.player.CustomProperties != null && PhotonNetwork.player.CustomProperties.Count > 0 && x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] != null && ((int)x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey]) == 1);
            var blueTeamCount = PhotonNetwork.playerList.Where(x => x.ID != PhotonNetwork.player.ID).Count(x => PhotonNetwork.player.CustomProperties != null && PhotonNetwork.player.CustomProperties.Count > 0 && x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] != null && ((int)x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey]) == 2);

            Debug.Log(string.Format("Total: {0} Red: {1}, Blue: {2}", PhotonNetwork.playerList.Length, redTeamCount, blueTeamCount));
            teamId = redTeamCount < blueTeamCount ? 1 : 2;

            // create 
            var localPlayerCustomProps = new ExitGames.Client.Photon.Hashtable();

            // Update the local custom props
            localPlayerCustomProps[PlayerManager3D.PlayerTeamPrefKey] = teamId;

            // Update the player custom props.
            PhotonNetwork.player.SetCustomProperties(localPlayerCustomProps);
        }

        return teamId;
    }

    protected void HaltGame()
    {
        if (ShipManager.instance != null)
        {
            var dontDestroyOnLoadObjects = ShipManager.instance.gameObject.scene.GetRootGameObjects();
            foreach (var dontDestroyOnLoadObject in dontDestroyOnLoadObjects)
            {
                if (PhotonNetwork.inRoom)
                {
                    ConsoleMsg(string.Format("Network Destroy {0} 'DontDestroy' game halted.", dontDestroyOnLoadObject.name));
                    PhotonNetwork.Destroy(dontDestroyOnLoadObject);
                }
                else
                {
                    ConsoleMsg(string.Format("Local Destroy {0} 'DontDestroy' game halted.", dontDestroyOnLoadObject.name));
                    Destroy(dontDestroyOnLoadObject);
                }
            }
        }
    }

    [PunRPC]
    protected void NetworkConsoleMsg(string msg)
    {
        Debug.Log(msg);
        ConsolePanel.Find("MessageStream").GetComponentInChildren<Text>().text += string.Format("\n{0}", msg);
    }

    [PunRPC]
    protected void NetworkFlagSpawnInUse(int spawnLocIndex)
    {
        instance.SpawnLocations[spawnLocIndex].Spawned(null);
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
