using Assets.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;
using System;
using Assets.Scripts.Controllers;
using System.Collections;
using System.Linq;
using Assets.Scripts.ScriptableObjects;

namespace Assets
{
    public class HudController : BaseController
    {
        public GameObject ObservedPlayer;

        public GameObject GameMenu;

        public GameObject MiniMap;

        public GameStateManager GameStateManager;

        public ScriptableObject PrevGameCompletionTime;

        public Vector3 MiniMapLargeScale;

        public float MiniMapLargeCamSize;

        public Text GameTimeRemainingText;

        public Sprite FireModeSingle;

        public Sprite FireModeCloud;

        public Sprite FireModeBeam;

        protected Vector3 MiniMapSmallScale;

        protected float MiniMapSmallCamSize;

        int lastBroadcastSeconds = int.MinValue;
        
        public CanvasRenderer PowerCoreHealthPanel;
        
        public Slider BlueCoreHealthSlider;
        
        public Slider RedCoreHealthSlider;

        [HideInInspector]
        protected Slider HealthSlider;

        [HideInInspector]
        protected Slider StaminaSlider;

        [HideInInspector]
        protected Slider EnergySlider;

        [HideInInspector]
        protected Text AmmoText;

        [HideInInspector]
        protected Image EquipmentIcon;

        [HideInInspector]
        protected Image LeftEquipmentFireMode;

        [HideInInspector]
        protected Image RightEquipmentFireMode;

        private StaminaComponent StaminaComponent;

        private HealthComponent HealthComponent;

        private EnergyComponent EnergyComponent;

        private HealthComponent redCoreHealthComponent;

        private HealthComponent blueCoreHealthComponent;

        private Camera MiniMapCamera;

        public void Awake()
        {
            if (PrevGameCompletionTime as IntVariable == null)
            {
                Debug.LogError("PrevGameCompletionTime property is not IntVariable compatible");
            }

            var stamSliderObj = FindChild("StaminaSlider");
            if (stamSliderObj != null)
            {
                StaminaSlider = stamSliderObj.GetComponent<Slider>();
            }

            var healthSliderObj = FindChild("HealthSlider");
            if (healthSliderObj != null)
            {

                HealthSlider = healthSliderObj.GetComponent<Slider>();
            }

            var energySliderObj = FindChild("EnergySlider");
            if (energySliderObj != null)
            {

                EnergySlider = energySliderObj.GetComponent<Slider>();
            }

            var equipIconObj = FindChild("EquipmentIcon");
            if (equipIconObj != null)
            {
                EquipmentIcon = equipIconObj.GetComponent<Image>();
            }

            var ammoText = FindChild("AmmoText");
            if (ammoText != null)
            {
                AmmoText = ammoText.GetComponent<Text>();
            }

            var leftEquipMode = FindChild("LeftEquipmentMode");
            if (leftEquipMode != null)
            {
                LeftEquipmentFireMode = leftEquipMode.GetComponent<Image>();
            }

            var rightEquipMode = FindChild("RightEquipmentMode");
            if (rightEquipMode != null)
            {
                RightEquipmentFireMode = rightEquipMode.GetComponent<Image>();
            }

            var redCoreHealth = FindChild("RedCoreHealth");
            if (redCoreHealth != null)
            {
                RedCoreHealthSlider = redCoreHealth.GetComponentInChildren<Slider>();
            }

            var blueCoreHealth = FindChild("BlueCoreHealth");
            if (blueCoreHealth != null)
            {
                BlueCoreHealthSlider = blueCoreHealth.GetComponentInChildren<Slider>();
            }

            var coreHealthPanel = FindChild("CoreHealthPanel");
            if (leftEquipMode != null)
            {
                PowerCoreHealthPanel = coreHealthPanel.GetComponentInChildren<CanvasRenderer>();
            }
        }

        public void Start()
        {
            // If the game menu is set.
            if (GameMenu != null)
            {
                // Hide it when we start.
                GameMenu.SetActive(false);
            }

            if (MiniMap != null)
            {
                MiniMapSmallScale = MiniMap.transform.localScale;
                MiniMapLargeScale = MiniMapLargeScale == Vector3.zero ? MiniMapSmallScale : MiniMapLargeScale;
                MiniMapCamera = Camera.allCameras.FirstOrDefault(x => x.name == "MiniMapCamera");
                if (MiniMapCamera != null)
                {
                    MiniMapSmallCamSize = MiniMapCamera.orthographicSize;
                    MiniMapLargeCamSize = MiniMapLargeCamSize == 0f ? MiniMapSmallCamSize : MiniMapLargeCamSize;
                }
            }

            if (GameStateManager != null)
            {
                PowerCoreHealthPanel.gameObject.SetActive(true);
                redCoreHealthComponent = GameStateManager.RedCoreHealthComponent;
                blueCoreHealthComponent = GameStateManager.BlueCoreHealthComponent;
            }
            else
            {
                PowerCoreHealthPanel.gameObject.SetActive(false);
                Debug.LogWarningFormat("GameStateManager property not set in '{0}'", this.name);
            }
        }

        public void Update()
        {
            if (ObservedPlayer == null)
            {
                SetObservedPlayer(PlayerManager3D.Get().LocalPlayerInstance);
            }

            if (StaminaSlider != null && StaminaComponent != null)
            {
                StaminaSlider.maxValue = StaminaComponent.MaxValue < 100 ? 100 : StaminaComponent.MaxValue;
                StaminaSlider.value = StaminaComponent.CurrentValue;
            }

            if (HealthSlider != null & HealthComponent != null)
            {
                HealthSlider.maxValue = HealthComponent.MaxValue < 100 ? 100 : HealthComponent.MaxValue;
                HealthSlider.value = HealthComponent.CurrentValue;
            }

            if (EnergySlider != null & EnergyComponent != null)
            {
                EnergySlider.maxValue = EnergyComponent.MaxValue < 100 ? 100 : EnergyComponent.MaxValue;
                EnergySlider.value = EnergyComponent.CurrentValue;
            }

            if (RedCoreHealthSlider != null & redCoreHealthComponent != null)
            {
                RedCoreHealthSlider.maxValue = redCoreHealthComponent.MaxValue;
                RedCoreHealthSlider.value = redCoreHealthComponent.CurrentValue;
            }

            if (BlueCoreHealthSlider != null & blueCoreHealthComponent != null)
            {
                BlueCoreHealthSlider.maxValue = blueCoreHealthComponent.MaxValue;
                BlueCoreHealthSlider.value = blueCoreHealthComponent.CurrentValue;
            }

            // If the game menu is not null
            if (GameMenu != null)
            {
                // And they pressed the Escape key.
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    // Toggle the menu active state.
                    GameMenu.SetActive(!GameMenu.GetActive());
                }
            }

            // If the game menu is not null
            if (MiniMap != null)
            {
                // And they pressed the Escape key.
                if (Input.GetKeyDown(KeyCode.M))
                {
                    // Toggle the menu active state.
                    var scale = MiniMap.transform.localScale;
                    if (scale == MiniMapSmallScale)
                    {
                        StartCoroutine(AnimateMiniMapTransition(MiniMapSmallScale, MiniMapLargeScale, MiniMapSmallCamSize, MiniMapLargeCamSize, Time.deltaTime * 3));
                    }
                    else
                    {
                        StartCoroutine(AnimateMiniMapTransition(MiniMapLargeScale, MiniMapSmallScale, MiniMapLargeCamSize, MiniMapSmallCamSize, Time.deltaTime * 3));
                    }
                }
            }

            if (FireModeSingle != null && FireModeCloud != null && FireModeBeam != null)
            {
                if (ObservedPlayer != null)
                {
                    var equipment = ObservedPlayer.GetComponent<EquipmentComponent>();
                    if (equipment != null)
                    {
                        if (RightEquipmentFireMode != null)
                        {
                            var weapon = equipment.LeftHand.GetComponentInChildren<Assets.Scripts.GameObjects.Weapon>();
                            if (weapon != null)
                            {
                                switch (weapon.CurrentFireMode)
                                {
                                    case Scripts.Enums.FireMode.Single:
                                        {
                                            LeftEquipmentFireMode.sprite = FireModeSingle;
                                            break;
                                        }
                                    case Scripts.Enums.FireMode.Cloud:
                                        {
                                            LeftEquipmentFireMode.sprite = FireModeCloud;
                                            break;
                                        }
                                    case Scripts.Enums.FireMode.Beam:
                                        {
                                            LeftEquipmentFireMode.sprite = FireModeBeam;
                                            break;
                                        }
                                }
                            }
                        }

                        if (RightEquipmentFireMode != null)
                        {
                            var weapon = equipment.RightHand.GetComponentInChildren<Assets.Scripts.GameObjects.Weapon>();
                            if (weapon != null)
                            {
                                switch (weapon.CurrentFireMode)
                                {
                                    case Scripts.Enums.FireMode.Single:
                                        {
                                            RightEquipmentFireMode.sprite = FireModeSingle;
                                            break;
                                        }
                                    case Scripts.Enums.FireMode.Cloud:
                                        {
                                            RightEquipmentFireMode.sprite = FireModeCloud;
                                            break;
                                        }
                                    case Scripts.Enums.FireMode.Beam:
                                        {
                                            RightEquipmentFireMode.sprite = FireModeBeam;
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
            }

            // If we are the master client.
            if (PhotonNetwork.isMasterClient)
            {
                // Get the game state manager
                var gameStateManager = FindObjectOfType<GameStateManager>();

                // If the game state manager is not null and a previous time has been set.
                if (PrevGameCompletionTime != null && ((IntVariable)PrevGameCompletionTime).Value > 0)
                {
                    // Calculate the remaining time based on the current time since game start.
                    var secondsRemaining = ((IntVariable)PrevGameCompletionTime).Value - (int)Time.timeSinceLevelLoad;
                    if (secondsRemaining == lastBroadcastSeconds)
                    {
                        return;
                    }

                    lastBroadcastSeconds = secondsRemaining;
                    var photonView = this.GetComponent<PhotonView>();
                    if (photonView != null && PhotonNetwork.inRoom)
                    {
                        PhotonNetwork.RPC(photonView, "UpdateTimeRemaining", PhotonNetworkSettings.EventTarget, false, new object[] { secondsRemaining });
                    }
                    else
                    {
                        UpdateTimeRemaining(secondsRemaining);
                    }
                }
            }

        }

        public void SetObservedPlayer(GameObject player = null)
        {
            if (ObservedPlayer == null)
            {
                if (player != null)
                { 
                    ObservedPlayer = player;
                }
                else
                {
                    Debug.LogWarning("No observed player in the HUD controller and no Local player to use instead.");
                }
            }

            if (ObservedPlayer != null)
            {
                StaminaComponent = ObservedPlayer.GetComponent<StaminaComponent>();
                HealthComponent = ObservedPlayer.GetComponent<HealthComponent>();
                EnergyComponent = ObservedPlayer.GetComponent<EnergyComponent>();
            }
        }

        [PunRPC]
        public void UpdateTimeRemaining(int seconds)
        {
            if (GameTimeRemainingText != null)
            {
                var timeSpanRemaining = new TimeSpan(0, 0, seconds);
                if (timeSpanRemaining.TotalMinutes < 1)
                {
                    GameTimeRemainingText.text = string.Format("TIME: {0}", timeSpanRemaining.TotalSeconds);
                }
                else
                {
                    GameTimeRemainingText.text = string.Format("TIME: {0}:{1}", timeSpanRemaining.Minutes, timeSpanRemaining.Seconds);
                }

                GameTimeRemainingText.enabled = true;
            }
        }

        // every 2 seconds perform the print()
        private IEnumerator AnimateMiniMapTransition(Vector3 from, Vector3 to, float camFrom, float camTo, float time)
        {
            float elapsedTime = 0f;
            while (elapsedTime < time)
            {
                MiniMap.transform.localScale = Vector3.Lerp(from, to, (elapsedTime / time));
                if (MiniMapCamera != null)
                {
                    MiniMapCamera.orthographicSize = Mathf.Lerp(camFrom, camTo, (elapsedTime / time));
                }
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            MiniMap.transform.localScale = to;
            if (MiniMapCamera != null)
            {
                MiniMapCamera.orthographicSize = camTo;
            }
        }
    }
}