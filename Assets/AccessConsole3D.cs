using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using ExitGames.Client.Photon;
using Assets.Scripts.Components;
using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AccessConsole3D : Photon.PunBehaviour, IConsole
{
    public string AssociatedScene;

    public Canvas HudOverlay;

    public int AllowTeamAccess;

    public bool LockOnSuccess;

    public float LockOutTime = 15f;

    public bool Locked = false;

    public bool DeactivateAfterResult = true;

    public ScriptableObject DifficultySettings;

    public List<EventTriggerVariable> OnConsoleAccessTriggers;

    public List<EventTriggerVariable> OnConsoleExitTriggers;

    /// <summary>
    /// list of items a player or a child GO of the player must have in order to access this console.
    /// </summary>
    public List<string> PlayerAccessRequirements;

    public Material LockMaterial;

    public List<MeshRenderer> Monitors;

    public List<Light> Lights;

    public List<AudioClip> AlertClips;

    public int MaterialIndex;

    public string ConsoleAccessKey = "R";

    protected Material OldMaterial;

    protected float CurrentLockoutTime = 0;

    protected bool InUse = false;

    protected int FailureCount = 0;

    protected Canvas ConsoleToolTip;

    protected bool ToolTipShowing;

    /// <summary>
    /// The console result ranges from 0 (fail) to 1 (pass)
    /// </summary>
    protected float ConsoleResultValue = 0;

    protected bool LockTimerActive = false;

    bool unlockFired = false;

    public IPlayerController playerReference;

    public virtual void Awake()
    {
        var consoleTip = transform.Find("ConsoleAccessTip");
        if (consoleTip != null)
        {
            // Trigger the animation to display the access tip.
            var consoleTipCanv = transform.Find("ConsoleAccessTip").GetComponent<Canvas>();
            if (consoleTipCanv != null)
            {
                ConsoleToolTip = consoleTipCanv;
                FitConsoleTip(consoleTipCanv.GetComponentInChildren<Text>());
            }
        }
    }

    public virtual void Start()
    {
        ApplyLockFromRoom();
    }

    public virtual void Update()
    {
        // If the conosole is locked.
        if (this.Locked)
        {
            // If the lock timer is active
            if (this.LockTimerActive)
            {
                // Increment the lockout time so that it will automatically unlock when complete.
                this.CurrentLockoutTime += Time.deltaTime;
            }

            // If the lockout time has reached its limit, publish the unlocked state once.
            if (this.LockTimerActive && this.CurrentLockoutTime >= this.LockOutTime && !unlockFired)
            {
                unlockFired = true;
                if (!PhotonNetwork.inRoom || this.photonView == null || this.photonView.isMine)
                {
                    RequestConsoleLock(false, false);
                }
            }
        }
    }

    public virtual int GetFailureCount()
    {
        return FailureCount;
    }

    public virtual void SetConsoleResult(float passMark, bool deactivateOverride = false)
    {
        ConsoleResultValue = passMark;
        if (ConsoleResultValue <= 0)
        {
            RequestConsoleLock(true, true);

            FailureCount++;
        }
        else
        {
            ConsoleComplete();
            FailureCount = 0;
        }

        // If we still have refrence to the player then
        if (playerReference != null && (DeactivateAfterResult || deactivateOverride))
        {
            // Deactivate after result.
            Deactivate(playerReference);
        }
    }

    public virtual PhotonView GetPhotonView()
    {
        return this.photonView;
    }

    public virtual void OnTriggerStay(Collider col)
    {
        // If the tool tip is not showing.
        if (!ToolTipShowing)
        {
            var player = col.GetComponentInParent<PlayerController3D>();
            if (player != null)
            {
                // Adjust the console access text tip based on player and console state
                if (ConsoleToolTip != null)
                {
                    var consoleAccessText = ConsoleToolTip.GetComponentInChildren<Text>();
                    if (consoleAccessText != null)
                    {
                        // Deteremine if the team player can access this console, and adjust the access tip accordingly.
                        var accessText = 
                            this.AllowPlayerAccess() ? 
                                this.Locked ? 
                                    "!! CONSOLE LOCKED !!" : 
                                    this.PlayerMeetsAccessRestrictions(player) ? 
                                        string.Format("Press \"{0}\" to activate", ConsoleAccessKey) :
                                        string.Format("No {0}", PlayerAccessRequirements.ToArray()) : 
                            "!! ACCESS DENIED !!";
                        consoleAccessText.text = accessText;
                        FitConsoleTip(consoleAccessText);
                    }
                }

                var pv = player.gameObject.GetComponent<PhotonView>();
                if (pv.isMine || !PhotonNetwork.inRoom)
                {
                    player.CurrentConsole = this;
                    ToggleTip(true);
                }
            }
        }
    }

    public virtual void OnTriggerExit(Collider col)
    {
        if (ToolTipShowing)
        {
            var player = col.GetComponentInParent<PlayerController3D>();
            if (player != null)
            {
                var pv = player.gameObject.GetComponent<PhotonView>();
                if (pv.isMine || !PhotonNetwork.inRoom)
                {
                    player.CurrentConsole = null;
                    ToggleTip(false);
                }
            }
        }
    }

    public virtual bool IsActive()
    {
        return InUse;
    }

    public virtual bool IsLocked()
    {
        return Locked;
    }

    public virtual void Activate(IPlayerController player)
    {
        ConsoleResultValue = 0;
        playerReference = player;
        // If the assoc scene is not set.
        if (string.IsNullOrEmpty(this.AssociatedScene))
        {
            // Throw a warning.
            Debug.LogWarning(string.Format("No Associated Scene Set for '{0}'", transform.name));
            return;
        }

        // If the player is allowed access, the console is not in use and has not been locked.
        if (this.AllowPlayerAccess() && !this.IsActive() && !this.Locked && this.PlayerMeetsAccessRestrictions(player))
        {
            // If there are any access triggers
            if (OnConsoleAccessTriggers.Any())
            {
                // For each trigger.
                foreach (var accessTrigger in OnConsoleAccessTriggers)
                {
                    // Invoke the trigger.
                    accessTrigger.TriggerEvent.Invoke(new FloatTrigger { value = 0.1f, triggeredBy = this.gameObject });
                }
            }

            // Mark the console as in use.
            InUse = true;
            GameManager3D.GrantConsoleAccess(this);

            // Update the player to stop them moving, firing.
            player.SetDisableActions(true);
            player.SetDisableMovement(true);

            // Toggle the HUD so that it doesn't show over the minigame.
            ToggleHud(false);

            // Load the assoc. minigame scene.
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(this.AssociatedScene, LoadSceneMode.Additive);
        }
    }

    public virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var sceneGameManager = scene.GetRootGameObjects().ToList().FirstOrDefault(x => x.name == "GameManager");
        if (sceneGameManager != null)
        {
            var consoleSceneController = sceneGameManager.GetComponent<IConsoleSceneController>();
            if (consoleSceneController != null)
            {
                consoleSceneController.InitaliseMiniGame(this);
            }
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public virtual void Deactivate(IPlayerController player)
    {
        // If there are any completeion conditions
        if (OnConsoleExitTriggers.Any())
        {
            // Toggle the boolean flag when the exercise has been completed.
            foreach (var completionTriggers in OnConsoleExitTriggers)
            {
                completionTriggers.TriggerEvent.Invoke(new FloatTrigger { value = ConsoleResultValue, triggeredBy = this.gameObject });
            }
        }

        // If the assoc scene is not set.
        if (string.IsNullOrEmpty(this.AssociatedScene))
        {
            // Throw a warning.
            Debug.LogWarning(string.Format("No Associated Scene Set for '{0}'", transform.name));
            return;
        }

        // Set the console as available for use.
        InUse = false;
        GameManager3D.RevokeConsoleAccess(this);

        // Re-enable the player actions.
        player.SetDisableActions(false);
        player.SetDisableMovement(false);

        // Toggle the HUD to be visible 
        ToggleHud(true);

        // If the tooltip is showing.
        if (ToolTipShowing)
        {
            // Hide it.
            ToggleTip(false);
        }

        // Unload the scene.
        SceneManager.UnloadSceneAsync(this.AssociatedScene);
        playerReference = null;
    }

    public virtual IPlayerController GetActivePlayer()
    {
        return playerReference;
    }

    public virtual string GetAssociatedScene()
    {
        return this.AssociatedScene;
    }

    [PunRPC]
    public virtual void EndGame(int winningTeam)
    {
        //// GameManager3D.EndGame(winningTeam);
    }

    public virtual void ConsoleComplete()
    {
        // If there are any completeion conditions
        if (LockOnSuccess)
        {
            RequestConsoleLock(true, false);
        }
    }

    public virtual void RequestConsoleLock(bool locked, bool withTimer)
    {
        ApplyLockState(locked, withTimer);

        if (!PhotonNetwork.inRoom || PhotonNetwork.room == null)
        {
            return;
        }

        var props = new Hashtable();
        props[LockPropertyKey()] = locked ? (withTimer ? 2 : 1) : 0;
        PhotonNetwork.room.SetCustomProperties(props);
    }

    public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged != null && propertiesThatChanged.ContainsKey(LockPropertyKey()))
        {
            ApplyEncodedLockState((int)propertiesThatChanged[LockPropertyKey()]);
        }
    }

    [PunRPC]
    public virtual void LockConsole(bool withTimer = true)
    {
        ApplyLockState(true, withTimer);
    }

    [PunRPC]
    public virtual void UnlockConsole()
    {
        ApplyLockState(false, false);
    }

    protected virtual void ApplyLockState(bool locked, bool withTimer)
    {
        this.Locked = locked;
        this.LockTimerActive = locked && withTimer;
        this.CurrentLockoutTime = 0;
        unlockFired = false;
        ChangeMaterial();
    }

    string LockPropertyKey()
    {
        return "CL_" + gameObject.name;
    }

    void ApplyLockFromRoom()
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.room == null || PhotonNetwork.room.CustomProperties == null)
        {
            return;
        }

        var key = LockPropertyKey();
        if (PhotonNetwork.room.CustomProperties.ContainsKey(key))
        {
            ApplyEncodedLockState((int)PhotonNetwork.room.CustomProperties[key]);
        }
    }

    void ApplyEncodedLockState(int encoded)
    {
        if (encoded <= 0)
        {
            ApplyLockState(false, false);
        }
        else
        {
            ApplyLockState(true, encoded == 2);
        }
    }

    protected virtual void ToggleHud(bool show)
    {
        if (HudOverlay != null)
        {
            HudOverlay.gameObject.SetActive(show);
        }
    }

    protected virtual bool AllowPlayerAccess()
    {
        if (!PhotonNetwork.inRoom || this.AllowTeamAccess == 0)
        {
            return true;
        }

        if (PhotonNetwork.player == null || PhotonNetwork.player.CustomProperties == null || !PhotonNetwork.player.CustomProperties.ContainsKey(PlayerManager3D.PlayerTeamPrefKey))
        {
            return false;
        }

        var playerTeamId = (int)PhotonNetwork.player.CustomProperties[PlayerManager3D.PlayerTeamPrefKey];
        return playerTeamId == this.AllowTeamAccess;
    }

    protected virtual bool PlayerMeetsAccessRestrictions(IPlayerController player)
    {
        var playersAccessComponents = player.GetTransform().gameObject.GetComponentsInChildren<ConsoleAccessRequirement>();
        var agregatedPlayerComponents = new List<string>();
        if (playersAccessComponents.Any())
        {
            agregatedPlayerComponents = playersAccessComponents.SelectMany(x => x.AccessRequirements).ToList();
        }

        // Set the default result based on if we have any requirements.
        var result = !PlayerAccessRequirements.Any();

        // if we have requirements and the player has requirements components.
        if (PlayerAccessRequirements.Any() && agregatedPlayerComponents.Any())
        {
            // test to see if each required component is meet by the player.
            foreach (var accesReqType in PlayerAccessRequirements)
            {
                // The player cant access the console
                result = agregatedPlayerComponents.Contains(accesReqType);
            }
        }

        return result;
    }

    protected virtual void FitConsoleTip(Text label)
    {
        if (label == null || ConsoleToolTip == null)
        {
            return;
        }

        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.alignment = TextAnchor.MiddleCenter;

        var textRect = label.rectTransform;
        var characterCount = string.IsNullOrEmpty(label.text) ? 0 : label.text.Length;
        var preferred = 0f;
        if (label.font != null)
        {
            preferred = label.preferredWidth;
        }

        var width = ConsoleTipLayout.TextWidth(preferred, characterCount);
        if (width < textRect.sizeDelta.x)
        {
            width = textRect.sizeDelta.x;
        }

        var height = textRect.sizeDelta.y;
        if (height < label.fontSize + 8f)
        {
            height = label.fontSize + 8f;
        }

        textRect.sizeDelta = new Vector2(width, height);

        var canvasRect = ConsoleToolTip.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            return;
        }

        var panel = textRect.parent as RectTransform;
        var panelScaleX = panel != null ? panel.localScale.x : 1f;
        var canvasWidth = ConsoleTipLayout.CanvasWidth(width, textRect.localScale.x, panelScaleX, canvasRect.sizeDelta.x);
        canvasRect.sizeDelta = new Vector2(canvasWidth, canvasRect.sizeDelta.y);
    }

    protected virtual void ToggleTip(bool showing)
    {
        // If we are trying to toggle to a different state.
        if (ToolTipShowing != showing)
        {
            // Make sure we can access the console tool tip.
            if (ConsoleToolTip != null)
            {
                // Make sure we can access the animation
                var consoleAnim = ConsoleToolTip.GetComponent<Animation>();
                if (consoleAnim != null)
                {
                    // Adjust the ToolTipShowing property.
                    ToolTipShowing = showing;

                    // Play the tool tip animation.
                    consoleAnim.Play(showing ? "ConsoleTip" : "ConsoleTipOut");
                }
            }
        }
    }

    protected virtual void ChangeMaterial()
    {
        // If we have a lockout material and monitors
        if (LockMaterial != null && Monitors.Any())
        {
            // Switch out the material.
            foreach (var meshRenderer in Monitors)
            {
                if (meshRenderer != null)
                {
                    if (meshRenderer.materials != null && meshRenderer.materials.Any())
                    {
                        // Get the current materials.
                        var currentMats = meshRenderer.materials;

                        // Determine if the current material is the new material.
                        var currentMatIsNew = currentMats[MaterialIndex].name.StartsWith(LockMaterial.name);

                        // If the current material is not the new material and we want to apply the new material.
                        if (!currentMatIsNew && Locked)
                        {
                            // Store the old material.
                            OldMaterial = currentMats[MaterialIndex];

                            // Update to the new material.
                            currentMats[MaterialIndex] = LockMaterial;
                        }
                        else
                        {
                            // Providing we have an old material stored.
                            if (OldMaterial != null)
                            {
                                // Set the current material to be the original material.
                                currentMats[MaterialIndex] = OldMaterial;
                            }
                        }

                        // Reset the current materials.
                        meshRenderer.materials = currentMats;

                        ChangeLighting(Locked ? Color.red : Color.green);
                    }
                }
            }

            // unstore the old material.
            if (!Locked)
            {
                OldMaterial = null;
            }
        }
    }

    protected virtual void ChangeLighting(Color color)
    {
        // Change the monitor light colour
        foreach (var currentLight in Lights)
        {
            currentLight.color = color;    
        }
    }

    public virtual ScriptableObject GetDifficultySettings()
    {
        return DifficultySettings;
    }
}
