using System;
using System.Collections;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Components;
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

    Text[] respawnDigits;

    string respawnReadout = string.Empty;

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

            RestoreSpawnEnergy(player);
            StartCoroutine(RestoreEnergyAfterSpawn(player));
        }

        LocalPlayerInstance = player;
        var hud = FindObjectOfType<Assets.HudController>();
        if (hud != null)
        {
            hud.SetObservedPlayer(player);
        }
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

    static void RestoreSpawnEnergy(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        var energy = player.GetComponent<EnergyComponent>();
        if (energy == null)
        {
            var view = player.GetComponent<PhotonView>();
            if (view != null && PhotonNetwork.inRoom && !view.isMine)
            {
                return;
            }

            energy = player.AddComponent<EnergyComponent>();
            energy.MaxValue = 100f;
        }

        energy.CurrentValue = SpawnVitals.RestoredEnergy(energy.CurrentValue, energy.MaxValue);
    }

    IEnumerator RestoreEnergyAfterSpawn(GameObject player)
    {
        yield return null;
        RestoreSpawnEnergy(player);
        if (player == null)
        {
            yield break;
        }

        var hud = FindObjectOfType<Assets.HudController>();
        if (hud != null)
        {
            hud.SetObservedPlayer(player);
        }
    }

    protected void HideRespawnTimerFx()
    {
        if (RespawnTimerText != null && RespawnTimerText.enabled)
        {
            RespawnTimerText.enabled = false;
        }

        if (respawnDigits != null)
        {
            for (var i = 0; i < respawnDigits.Length; i++)
            {
                if (respawnDigits[i] != null)
                {
                    respawnDigits[i].enabled = false;
                }
            }
        }

        respawnReadout = string.Empty;
    }

    protected void UpdateRespawnTimerFx()
    {
        if (RespawnTimerText == null)
        {
            Debug.Log("No respawn timer text object set.");
            return;
        }

        EnsureRespawnReadout();
        RespawnTimerText.text = SpaceRaceCopy.RespawnLabel;
        if (!RespawnTimerText.enabled)
        {
            RespawnTimerText.enabled = true;
        }

        var formatted = SpaceRaceCopy.FormatRespawnCountdown(CurrentRespawnTimer);
        if (formatted == respawnReadout)
        {
            return;
        }

        respawnReadout = formatted;
        for (var i = 0; i < respawnDigits.Length; i++)
        {
            var digit = respawnDigits[i];
            digit.enabled = true;
            var character = formatted[i];
            digit.text = i == 0 && character == '0' ? string.Empty : character.ToString();
        }
    }

    void EnsureRespawnReadout()
    {
        if (respawnDigits != null || RespawnTimerText == null)
        {
            return;
        }

        var label = RespawnTimerText;
        var labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0.5f, 1f);
        labelRect.pivot = new Vector2(1f, 0.5f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = new Vector2(-8f, 0f);
        label.alignment = TextAnchor.MiddleRight;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.text = SpaceRaceCopy.RespawnLabel;

        var fontSize = label.fontSize > 0 ? label.fontSize : 110;
        var digitWidth = CharacterAdvance(label.font, fontSize, label.fontStyle, '0');
        var dotWidth = CharacterAdvance(label.font, fontSize, label.fontStyle, '.');
        if (dotWidth < digitWidth * 0.2f)
        {
            dotWidth = digitWidth * 0.35f;
        }

        float[] widths = { digitWidth, digitWidth, dotWidth, digitWidth, digitWidth };
        var total = 0f;
        for (var i = 0; i < widths.Length; i++)
        {
            total += widths[i];
        }

        var row = new GameObject("RespawnTimerValue");
        row.transform.SetParent(label.transform.parent, false);
        var rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.pivot = new Vector2(0f, 0.5f);
        rowRect.anchoredPosition = new Vector2(8f, 0f);
        rowRect.sizeDelta = new Vector2(total, fontSize * 1.4f);

        respawnDigits = new Text[widths.Length];
        var x = 0f;
        for (var i = 0; i < widths.Length; i++)
        {
            var digitObject = new GameObject("Digit" + i);
            digitObject.transform.SetParent(row.transform, false);
            var digitRect = digitObject.AddComponent<RectTransform>();
            digitRect.anchorMin = new Vector2(0f, 0f);
            digitRect.anchorMax = new Vector2(0f, 1f);
            digitRect.pivot = new Vector2(0.5f, 0.5f);
            digitRect.anchoredPosition = new Vector2(x + widths[i] * 0.5f, 0f);
            digitRect.sizeDelta = new Vector2(widths[i], 0f);

            var digit = digitObject.AddComponent<Text>();
            digit.font = label.font;
            digit.fontSize = fontSize;
            digit.fontStyle = label.fontStyle;
            digit.color = label.color;
            digit.alignment = TextAnchor.MiddleCenter;
            digit.horizontalOverflow = HorizontalWrapMode.Overflow;
            digit.verticalOverflow = VerticalWrapMode.Overflow;
            digit.raycastTarget = false;
            digit.enabled = false;
            respawnDigits[i] = digit;
            x += widths[i];
        }
    }

    static float CharacterAdvance(Font font, int size, FontStyle style, char character)
    {
        var fallback = character == '.' ? size * 0.28f : size * 0.62f;
        if (font == null)
        {
            return fallback;
        }

        font.RequestCharactersInTexture("0123456789.", size, style);
        CharacterInfo info;
        if (!font.GetCharacterInfo(character == '.' ? '.' : '0', out info, size, style))
        {
            return fallback;
        }

        if (character == '.')
        {
            return info.advance > 1f ? info.advance : fallback;
        }

        var widest = info.advance;
        for (var c = '0'; c <= '9'; c++)
        {
            CharacterInfo digitInfo;
            if (font.GetCharacterInfo(c, out digitInfo, size, style) && digitInfo.advance > widest)
            {
                widest = digitInfo.advance;
            }
        }

        return widest > 1f ? widest : fallback;
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
