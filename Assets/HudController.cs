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

        Image healthFill;

        Image staminaFill;

        Image energyFill;

        GameObject howToPanel;

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
            if (coreHealthPanel != null)
            {
                PowerCoreHealthPanel = coreHealthPanel.GetComponentInChildren<CanvasRenderer>();
            }

            healthFill = FillImage(HealthSlider);
            staminaFill = FillImage(StaminaSlider);
            energyFill = FillImage(EnergySlider);
        }

        public void Start()
        {
            // If the game menu is set.
            if (GameMenu != null)
            {
                // Hide it when we start.
                GameMenu.SetActive(false);
                EnsurePauseActions();
            }

            EnsureHudReadability();

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

            UpdateVitalColors();

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
                    if (!GameMenu.activeSelf && howToPanel != null)
                    {
                        howToPanel.SetActive(false);
                    }
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

            if (ObservedPlayer != null)
            {
                var equipment = ObservedPlayer.GetComponent<EquipmentComponent>();
                if (equipment != null)
                {
                    ApplyHandIcon(LeftEquipmentFireMode, equipment.LeftHand);
                    ApplyHandIcon(RightEquipmentFireMode, equipment.RightHand);
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
                    if (secondsRemaining < 0)
                    {
                        secondsRemaining = 0;
                    }

                    if (secondsRemaining != lastBroadcastSeconds)
                    {
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
                GameTimeRemainingText.text = SpaceRaceCopy.FormatTimeRemaining(seconds);
                GameTimeRemainingText.enabled = true;
            }
        }

        static void ApplyHandIcon(Image target, GameObject hand)
        {
            if (target == null)
            {
                return;
            }

            var held = hand == null ? null : hand.GetComponentInChildren<Assets.Scripts.GameObjects.Equipment>();
            var sprite = HandIcons.ForEquipment(held);
            target.preserveAspect = true;
            target.color = Color.white;
            target.enabled = sprite != null;
            if (sprite != null && target.sprite != sprite)
            {
                target.sprite = sprite;
            }
        }

        static Image FillImage(Slider slider)
        {
            if (slider == null || slider.fillRect == null)
            {
                return null;
            }

            return slider.fillRect.GetComponent<Image>();
        }

        void EnsureHudReadability()
        {
            TintFill(healthFill, SpaceRaceTheme.Health);
            TintFill(staminaFill, SpaceRaceTheme.Stamina);
            TintFill(energyFill, SpaceRaceTheme.Energy);
            TintFill(FillImage(RedCoreHealthSlider), SpaceRaceTheme.RedTeam);
            TintFill(FillImage(BlueCoreHealthSlider), SpaceRaceTheme.BlueTeam);
            EnsureCoreLabel(RedCoreHealthSlider, "RED CORE", SpaceRaceTheme.RedTeam);
            EnsureCoreLabel(BlueCoreHealthSlider, "BLUE CORE", SpaceRaceTheme.BlueTeam);
            EnsureSlotLabel(LeftEquipmentFireMode, "LEFT");
            EnsureSlotLabel(RightEquipmentFireMode, "RIGHT");
            EnsureMapHint();
        }

        void UpdateVitalColors()
        {
            TintFill(staminaFill, SpaceRaceTheme.Stamina);
            TintFill(energyFill, SpaceRaceTheme.Energy);

            if (healthFill == null || HealthComponent == null || HealthComponent.MaxValue <= 0)
            {
                TintFill(healthFill, SpaceRaceTheme.Health);
                return;
            }

            var ratio = HealthComponent.CurrentValue / HealthComponent.MaxValue;
            if (ratio > 0f && ratio < 0.25f)
            {
                var pulse = Mathf.PingPong(Time.time, 0.45f);
                healthFill.color = Color.Lerp(SpaceRaceTheme.Health, Color.white, pulse);
            }
            else
            {
                healthFill.color = SpaceRaceTheme.Health;
            }
        }

        static void TintFill(Image fill, Color color)
        {
            if (fill != null)
            {
                fill.color = color;
            }
        }

        static void EnsureCoreLabel(Slider slider, string label, Color color)
        {
            if (slider == null)
            {
                return;
            }

            var parent = slider.transform.parent != null ? slider.transform.parent : slider.transform;
            var existing = parent.Find("CoreLabel");
            Text text;
            if (existing == null)
            {
                text = SpaceRaceWidgets.CreateText(parent, "CoreLabel", label, 12, color, TextAnchor.MiddleCenter);
                var rect = text.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 2f);
                rect.sizeDelta = new Vector2(0f, 18f);
            }
            else
            {
                text = existing.GetComponent<Text>();
            }

            if (text != null)
            {
                text.text = label;
                text.color = color;
            }
        }

        static void EnsureSlotLabel(Image icon, string label)
        {
            if (icon == null)
            {
                return;
            }

            var existing = icon.transform.Find("SlotLabel");
            Text text;
            if (existing == null)
            {
                text = SpaceRaceWidgets.CreateText(icon.transform, "SlotLabel", label, 11, SpaceRaceTheme.Text, TextAnchor.MiddleCenter);
                var rect = text.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(0f, 14f);
            }
            else
            {
                text = existing.GetComponent<Text>();
            }

            if (text != null)
            {
                text.text = label;
                text.color = SpaceRaceTheme.Text;
            }
        }

        void EnsureMapHint()
        {
            if (MiniMap == null || MiniMap.transform.Find("MapHint") != null)
            {
                return;
            }

            var text = SpaceRaceWidgets.CreateText(MiniMap.transform, "MapHint", "M", 14, SpaceRaceTheme.Teal, TextAnchor.MiddleCenter);
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-4f, 4f);
            rect.sizeDelta = new Vector2(24f, 18f);
        }

        void EnsurePauseActions()
        {
            if (GameMenu == null || GameMenu.transform.Find("SpaceRacePauseActions") != null)
            {
                return;
            }

            var column = new GameObject("SpaceRacePauseActions", typeof(RectTransform));
            column.transform.SetParent(GameMenu.transform, false);
            var columnRect = column.GetComponent<RectTransform>();
            columnRect.anchorMin = new Vector2(1f, 0.5f);
            columnRect.anchorMax = new Vector2(1f, 0.5f);
            columnRect.pivot = new Vector2(0f, 0.5f);
            columnRect.anchoredPosition = new Vector2(16f, 0f);
            columnRect.sizeDelta = new Vector2(200f, 180f);

            var resume = SpaceRaceWidgets.CreateButton(column.transform, "Resume", "Resume", () =>
            {
                GameMenu.SetActive(false);
                if (howToPanel != null)
                {
                    howToPanel.SetActive(false);
                }
            });
            PlacePauseButton(resume, 60f);

            var howTo = SpaceRaceWidgets.CreateButton(column.transform, "HowToPlay", "How to play", ToggleHowToPlay);
            PlacePauseButton(howTo, 0f);

            var menuButtons = GameMenu.GetComponentsInChildren<Button>(true);
            for (var i = 0; i < menuButtons.Length; i++)
            {
                if (menuButtons[i].gameObject.name != "Quit")
                {
                    continue;
                }

                var quitLabel = menuButtons[i].GetComponentInChildren<Text>();
                if (quitLabel != null)
                {
                    quitLabel.text = "Leave match";
                }
            }
        }

        static void PlacePauseButton(Button button, float y)
        {
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(190f, 44f);
        }

        void ToggleHowToPlay()
        {
            if (howToPanel == null)
            {
                var parent = GameMenu.transform.parent != null ? GameMenu.transform.parent : GameMenu.transform;
                howToPanel = SpaceRaceWidgets.CreateHowToCard(parent, ToggleHowToPlay);
                return;
            }

            howToPanel.SetActive(!howToPanel.activeSelf);
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