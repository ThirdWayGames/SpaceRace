using System.Linq;
using Assets.Scripts.Components;
using Assets.Scripts.Enums;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(HealthComponent), typeof(MovementComponent), typeof(MouseRotationComponent))]
    public class PlayerController3D : BasePlayerController3D, IDisposeCallback
    {
        /// <summary>
        /// The current console
        /// </summary>
        public IConsole CurrentConsole;

        public AudioSource RunningSound;

        public ForceMode CurrentForceMode = ForceMode.Force;

        public float RespawnTimer = 15f;

        private HealthComponent HealthComponent;

        public void Awake()
        {
            if (this.GetComponent<PhotonView>() != null && this.GetComponent<PhotonView>().isMine)
            {
                PlayerManager3D.Get().SetLocalPlayer(this.gameObject);
            }

            /*
             * Not sure why this was in here, I think it is to enable us to load non-addative and still have players in the scene.
             * Taken it out for the time being.
             */
            //// DontDestroyOnLoad(this.gameObject);

            HealthComponent = GetComponent<HealthComponent>();

            InitialiseClientSpawn();
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            // If this is not my photon view.
            if (PhotonNetwork.inRoom)
            {
                if (!this.GetComponent<PhotonView>().isMine)
                {
                    // Exit this method.
                    return;
                }
            }

            // We are near a console and have pressed the console key.
            if (CurrentConsole != null && Input.GetKeyDown(KeyCode.R))
            {
                if (!CurrentConsole.IsActive())
                {
                    CurrentConsole.Activate(this);
                }
                else
                {
                    CurrentConsole.Deactivate(this);
                }
            }

            // If they have suicided
            if (Input.GetKeyDown(KeyCode.K))
            {
                // Kill the player
                HealthComponent.CurrentValue = 0;
            }
        }

        /// <summary>
        /// Initialises the client spawn.
        /// </summary>
        public override void InitialiseClientSpawn()
        {
            // Set the camera to follow this player.
            if (Camera.main != null)
            {
                var mainCameraFollow = Camera.main.GetComponent<CameraFollow3D>();
                if (mainCameraFollow != null)
                {
                    mainCameraFollow.SetTarget(transform);
                }
            }

            // Get the minimap camera
            if (Camera.allCameras.Any())
            {
                var minimapCamera = Camera.allCameras.FirstOrDefault(x => x.name == "MiniMapCamera");
                if (minimapCamera != null)
                {
                    // Set the minimap camera to follow 
                    var camFollow3D = minimapCamera.GetComponent<CameraFollow3D>();
                    if (camFollow3D != null)
                    {
                        camFollow3D.SetTarget(transform);
                    }
                }
            }

            // Get the Fog of War visualiser
            var viewVisual = transform.Find("ViewVisualisation");
            if (viewVisual != null)
            {
                // Get the Mesh renderer
                var meshRend = viewVisual.GetComponent<MeshRenderer>();
                if (meshRend != null)
                {
                    // Turn it on for client spawned player.
                    meshRend.enabled = true;
                }

                var setShaderProp = viewVisual.GetComponent<SetShaderProp>();
                if (setShaderProp != null)
                {
                    // Turn it on for client spawned player.
                    setShaderProp.enabled = true;
                }
            }

            // If we are not in a room
            if (!PhotonNetwork.inRoom)
            {
                this.enabled = true;

                // Exit the method and dont perform initialisation.
                return;
            }

            // If this is not my photon view.
            if (!this.GetComponent<PhotonView>().isMine)
            {
                // Exit this method.
                return;
            }

            // Enable this script for client spawns.
            this.enabled = true;
            var fov = GetComponent<FieldOfView3D>();
            if (fov != null)
            {
                fov.enabled = true;
            }

            // Enable the tranform network monitoring.
            var audioListener = GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }

            // Enable the tranform network monitoring.
            var photonTransformView = GetComponent<PhotonTransformView>();
            if (photonTransformView != null)
            {
                photonTransformView.enabled = true;
            }

            // Enable the animation network monitoring.
            var photonAnimationView = GetComponent<PhotonAnimatorView>();
            if (photonAnimationView != null)
            {
                photonAnimationView.enabled = true;
            }
        }

        /// <summary>
        /// Called when another script would result in the destruction of this object.
        /// </summary>
        /// <param name="currentTime"></param>
        public void DisposeItem(float currentTime)
        {
            // We are near a console and have pressed the console key.
            if (CurrentConsole != null && CurrentConsole.IsActive())
            {
                CurrentConsole.Deactivate(this);
            }

            // Destroy the main clone.
            PlayerManager3D.Get().DespawnPlayer(this);
        }
    }
}
