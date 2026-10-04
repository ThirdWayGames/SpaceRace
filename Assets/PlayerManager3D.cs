using System;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Interfaces;

using UnityEngine;
using UnityEngine.UI;

public class PlayerManager3D : Photon.MonoBehaviour
{
    private static PlayerManager3D PlayerManager;

    public static PlayerManager3D Get()
    {
        if (!PlayerManager)
        {
            PlayerManager = FindObjectOfType<PlayerManager3D>() as PlayerManager3D;

            if (!PlayerManager)
            {
                Debug.LogError("There is no PlayerManager found in the scene");
            }
        }

        return PlayerManager;
    }

    public static string PlayerTeamPrefKey = "PlayerTeam";

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public GameObject LocalPlayerInstance;

    public int PlayerDeathCount = 0;

    [Tooltip("The number of game time mins bewteen the respawn timer increasing")]
    public int RespawnTimerIncreaseIterval = 5;

    [Tooltip("The time in seconds that the respawn timer increases for each interval")]
    public int DeathCost = 2;

    [Tooltip("The max time in seconds that the respawn can be")]
    public int MaxRespawnTime = 30;

    public float CurrentRespawnTimer = 0;

    public ScriptableObject ClonePathogenLoadout;

    public AudioListener MainAudioListener;

    public bool PlayerAlive = false;

    public bool ShowRespawnCalculations = false;

    public Button SpawnButton;

    public Text RespawnTimerText;

    protected float initialSpawnTime = 0f;

    protected int gameTimeInMins = 0;

    protected int calculatedDeathCost = 0;

    bool spawnRequested = false;

    bool spawnFailed = false;

    bool spawnRpcSent = false;

    public void SetLocalPlayer(GameObject localPlayer)
    {
        LocalPlayerInstance = localPlayer;
    }

    public GameObject GetLocalPlayer()
    {
        return LocalPlayerInstance;
    }

    public void TriggerRespawnTimer(float timer)
    {
        CurrentRespawnTimer = timer;
    }

    public void Awake()
    {
        if (SpawnButton == null)
        {
            Debug.Log("No Spawn Button found");
        }
    }

    public void Update()
    {
        gameTimeInMins = (int)Math.Round((Time.time - initialSpawnTime) / 60f);

        // If there is a main audio listener
        if (MainAudioListener != null)
        {
            // Disable it if the player is alive.
            MainAudioListener.enabled = !this.PlayerAlive;
        }

        if (!this.PlayerAlive)
        {
            if (this.CurrentRespawnTimer > 0)
            {
                this.CurrentRespawnTimer -= Time.deltaTime;
                UpdateRespawnTimerFx();
            }
            else
            {
                HideRespawnTimerFx();
                if (LocalPlayerInstance == null && !spawnRequested && !spawnFailed)
                {
                    SpawnPlayer();
                }
            }
        }
        else
        {
            spawnRequested = false;
            spawnFailed = false;
            // Hide the spawn button.
            if (SpawnButton != null)
            {
                SpawnButton.gameObject.SetActive(false);
            }
        }
    }

    public void SpawnPlayer()
    {
        if (PlayerAlive || LocalPlayerInstance != null)
        {
            spawnRequested = false;
            return;
        }

        if (spawnRequested)
        {
            return;
        }

        spawnRequested = true;
        RequestSpawn();
    }

    public void OnTeamAssigned()
    {
        if (!spawnRequested || spawnFailed || PlayerAlive || LocalPlayerInstance != null)
        {
            return;
        }

        RequestSpawn();
    }

    public void CompleteSpawn(int spawnLocIndex)
    {
        var locations = GameManager3D.instance != null ? GameManager3D.instance.SpawnLocations : null;
        if (locations == null || spawnLocIndex < 0 || spawnLocIndex >= locations.Length || locations[spawnLocIndex] == null)
        {
            FailSpawn();
            return;
        }

        FinishSpawn(locations[spawnLocIndex]);
    }

    public void FailSpawn()
    {
        spawnRequested = false;
        spawnRpcSent = false;
        spawnFailed = true;
        GameManager3D.ConsoleMsg("Spawn request failed");
    }

    void RequestSpawn()
    {
        GameManager3D.ConsoleMsg("Spawning Player");
        if (LocalPlayerInstance != null)
        {
            spawnRequested = false;
            return;
        }

        if (GameManager3D.instance == null)
        {
            FailSpawn();
            return;
        }

        if (!PhotonNetwork.inRoom)
        {
            var spawnLoc = GameManager3D.PickOfflineSpawn();
            if (spawnLoc == null)
            {
                GameManager3D.ConsoleMsg("Spawn Loc was null");
                FailSpawn();
                return;
            }

            FinishSpawn(spawnLoc);
            return;
        }

        var teamId = GameManager3D.GetTeamId();
        if (teamId == 0)
        {
            GameManager3D.ConsoleMsg("No Local Player Instance Found");
            GameManager3D.instance.AskMasterForTeam();
            return;
        }

        if (spawnRpcSent || GameManager3D.instance.photonView == null)
        {
            if (GameManager3D.instance.photonView == null)
            {
                FailSpawn();
            }
            return;
        }

        spawnRpcSent = true;
        GameManager3D.instance.photonView.RPC("RequestSpawnPoint", PhotonTargets.MasterClient, teamId);
    }

    void FinishSpawn(TeleportController spawnLoc)
    {
        GameManager3D.ConsoleMsg("No Local Player Instance Found");
        if (spawnLoc == null)
        {
            FailSpawn();
            return;
        }

        GameManager3D.ConsoleMsg(string.Format("{0} Spawning", PhotonNetwork.inRoom ? "Network" : "Local"));
        var marinePrefab = GameManager3D.instance.GetMarinePrefab();
        var player = PhotonNetwork.inRoom ? PhotonNetwork.Instantiate(marinePrefab.name, spawnLoc.GetSpawnPos(), spawnLoc.GetSpawnRot(), 0) : Instantiate(marinePrefab, spawnLoc.GetSpawnPos(), spawnLoc.GetSpawnRot());
        player.layer = LayerMask.NameToLayer("Player");

        if (initialSpawnTime == 0f)
        {
            initialSpawnTime = Time.time;
        }

        if (player != null)
        {
            var playerAnimController = player.GetComponentInChildren<PlayerAnimController>();
            if (playerAnimController != null)
            {
                playerAnimController.SetTeam(GameManager3D.GetTeamId());
            }

            var playerController = player.GetComponent<IPlayerController>();
            if (playerController != null)
            {
                if (ClonePathogenLoadout != null)
                {
                    playerController.SetPathogenLoadout(ClonePathogenLoadout);
                }
                else
                {
                    Debug.LogWarningFormat("No 'ClonePathogenLoadout' defined in the '{0}'", this.name);
                }

                spawnLoc.Spawned(playerController);
                PlayerAlive = true;
            }
        }

        LocalPlayerInstance = player;
        spawnRequested = false;
        spawnRpcSent = false;
        if (player == null)
        {
            spawnFailed = true;
        }
    }

    public void DespawnPlayer(PlayerController3D player, bool triggerRespawnTimer = true)
    {
        Debug.Log(string.Format("Despawing player '{0}' from scene", PhotonNetwork.player.NickName));

        // Get the player controller.
        if (player != null)
        {
            // Despawn the player.
            if (PhotonNetwork.inRoom && player.GetComponent<PhotonView>().isMine)
            {
                Debug.Log("Photon Destroy");
                PhotonNetwork.Destroy(player.GetComponent<PhotonView>());
            }
            else
            {
                Debug.Log("Destroy");
                Destroy(player.gameObject);
            }

            // Tell the player manager that the player is not alive.
            PlayerAlive = false;
            spawnRequested = false;
            spawnFailed = false;
            spawnRpcSent = false;

            // Increment the death count.
            PlayerDeathCount += 1;

            // Respawn the player.
            calculatedDeathCost = Mathf.Clamp((gameTimeInMins / RespawnTimerIncreaseIterval) * DeathCost, 0, MaxRespawnTime);

            TriggerRespawnTimer(player.RespawnTimer + calculatedDeathCost);

            // Reset the local player instance as its been destroyed.
            LocalPlayerInstance = null;
        }
    }

    protected void HideRespawnTimerFx()
    {
        if (RespawnTimerText != null)
        {
            if (RespawnTimerText.enabled)
            {
                RespawnTimerText.enabled = false;
            }
        }
    }

    protected void UpdateRespawnTimerFx()
    {
        if (RespawnTimerText == null)
        {
            Debug.Log("No respawn timer text object set.");
            return;
        }

        RespawnTimerText.text = string.Format("RESPAWING: {0:F}", Mathf.Clamp(CurrentRespawnTimer, 0, CurrentRespawnTimer));
        if (!RespawnTimerText.enabled)
        {
            RespawnTimerText.enabled = true;
        }
    }

    void OnGUI()
    {
        if (ShowRespawnCalculations)
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 20, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = Color.white;
            string text = string.Format("({0} - {1}/60) = {2} mins ({3} cost)", initialSpawnTime, Time.time, gameTimeInMins, calculatedDeathCost);
            GUI.Label(rect, text, style);
        }
    }
}
