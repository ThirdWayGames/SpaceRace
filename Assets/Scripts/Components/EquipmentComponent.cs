using Assets.Scripts.Enums;
using Assets.Scripts.GameObjects;
using Assets.Scripts.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Components
{
    [RequireComponent(typeof(PhotonView), typeof(GameObjectEntity))]
    public class EquipmentComponent : Photon.PunBehaviour
    {
        public List<EventTriggerVariable> TriggerEvents;

        /// <summary>
        /// The default item held in the Left hand.
        /// </summary>
        public ScriptableObject EquipmentLoadout;

        /// <summary>
        /// The Left hand spawn location
        /// </summary>
        public GameObject LeftHand;

        public bool LeftHandDisabled = false;

        public string LeftHandActionButton = "Fire1";

        public string LeftHandToggleButton = "Q";

        protected KeyCode LeftHandToggleCode;

        /// <summary>
        /// The Right hand spawn location
        /// </summary>
        public GameObject RightHand;

        public bool RightHandDisabled = false;

        public string RightHandActionButton = "Fire2";

        public string RightHandToggleButton = "E";

        protected KeyCode RightHandToggleCode;

        public void Awake()
        {
            LeftHandToggleCode = (KeyCode)Enum.Parse(typeof(KeyCode), LeftHandToggleButton);
            RightHandToggleCode = (KeyCode)Enum.Parse(typeof(KeyCode), RightHandToggleButton);
        }

        public void Start()
        {
            if (TriggerEvents.Any())
            {
                foreach (var trigger in TriggerEvents)
                {
                    trigger.TriggerEvent.AddListener(ListenerTriggered);
                }
            }

            if (EquipmentLoadout == null)
            {
                Debug.LogWarning(string.Format("Equipment loadout is null in the {0}'s equipment component.", this.gameObject.name));
            }

            if (LeftHand == null)
            {
                Debug.LogWarning(string.Format("Left hand is null in the {0}'s equipment component.", this.gameObject.name));
            }

            if (RightHand == null)
            {
                Debug.LogWarning(string.Format("Right hand is null in the {0}'s equipment component.", this.gameObject.name));
            }

            if (PhotonNetwork.inRoom)
            {
                // If this is not my photon view
                if (!this.gameObject.GetComponent<PhotonView>().isMine)
                {
                    // Get the entity manager
                    var entityManager = World.Active.GetExistingManager<EntityManager>();

                    // Get the entity.
                    var entity = this.gameObject.GetComponent<GameObjectEntity>().Entity;

                    // Remove the component from the entity.
                    entityManager.RemoveComponent(entity, GetType());

                    // Remove me as a component from the object as I will interfere with 
                    // network'd values observed by the photon view from the other client.
                    Destroy(this);
                    return;
                }
            }

            SpawnEquipment();
        }

        public void Update()
        {
            if (LeftHand != null)
            {
                ProcessEquipment(LeftHandDisabled, LeftHand, LeftHandToggleCode, LeftHandActionButton);
            }

            if (RightHand != null)
            {
                ProcessEquipment(RightHandDisabled, RightHand, RightHandToggleCode, RightHandActionButton);
            }
        }

        /// <summary>
        /// Gets the hand transform.
        /// </summary>
        /// <param name="handIndex">Index of the hand.</param>
        /// <returns></returns>
        public virtual Transform GetHandTransform(int handIndex)
        {
            return handIndex == 0 ? LeftHand.transform : RightHand.transform;
        }

        public void SpawnEquipment()
        {
            // Despawn equipment first.
            DespawnEquipment();

            var equipment = EquipmentLoadout as EquipmentLoadout;
            var teamComp = this.gameObject.GetComponentInParent<TeamComponent>();
            var teamId = teamComp == null ? 0 : teamComp.TeamIdentifier;

            // Spawn gun
            if (equipment.RightHandEquipment != null)
            {
                if (RightHand != null)
                {
                    // If there is nothing in the hand.
                    if (RightHand.transform.childCount == 0)
                    {
                        GameObject weapon;

                        // Spawn the default weapon
                        if (PhotonNetwork.inRoom)
                        {
                            weapon = PhotonNetwork.Instantiate(equipment.RightHandEquipment.name, RightHand.transform.position, RightHand.transform.rotation, 0, GenereateSpawnData(RightHand).ToOjectArray());
                        }
                        else
                        {
                            // Spwan locally and parent it to the hand.
                            weapon = Instantiate(equipment.RightHandEquipment, RightHand.transform.position, RightHand.transform.rotation);
                            weapon.transform.parent = RightHand.transform;
                        }

                        // Apply any mutations to the equiper defined by the item.
                        var equipmentMutationApplicator = weapon.GetComponent<EquipmentMutationApplicator>();
                        var mutationController = this.gameObject.GetComponentInParent<MutationController>();
                        if (equipmentMutationApplicator != null && mutationController != null)
                        {
                            mutationController.CureMutation(equipmentMutationApplicator.MutationsToRemoveWhenEquiped.Cast<BaseMutation>().ToList());
                            mutationController.RemoveImmunities(equipmentMutationApplicator.ImmunitiesToRemoveWhenEquiped.Cast<BaseMutation>().ToList());

                            mutationController.AddMutation(equipmentMutationApplicator.MutationsToApplyWhenEquiped.Cast<BaseMutation>().ToList(), teamId);
                            mutationController.AddImmunities(equipmentMutationApplicator.ImmunitiesToApplyWhenEquiped.Cast<BaseMutation>().ToList());
                        }
                    }
                }
            }

            if (equipment.LeftHandEquipment != null)
            {
                if (LeftHand != null)
                {
                    // If there is nothing in the hand.
                    if (LeftHand.transform.childCount == 0)
                    {
                        GameObject weapon;

                        // Spawn the default weapon
                        if (PhotonNetwork.inRoom)
                        {
                            weapon = PhotonNetwork.Instantiate(equipment.LeftHandEquipment.name, LeftHand.transform.position, LeftHand.transform.rotation, 0, GenereateSpawnData(LeftHand).ToOjectArray());
                        }
                        else
                        {
                            // Spwan locally and parent it to the hand.
                            weapon = Instantiate(equipment.LeftHandEquipment, LeftHand.transform.position, LeftHand.transform.rotation);
                            weapon.transform.parent = LeftHand.transform;
                        }

                        // Apply any mutations to the equiper defined by the item.
                        var equipmentMutationApplicator = weapon.GetComponent<EquipmentMutationApplicator>();
                        var mutationController = this.gameObject.GetComponentInParent<MutationController>();
                        if (equipmentMutationApplicator != null && mutationController != null)
                        {
                            mutationController.CureMutation(equipmentMutationApplicator.MutationsToRemoveWhenEquiped.Cast<BaseMutation>().ToList());
                            mutationController.RemoveImmunities(equipmentMutationApplicator.ImmunitiesToRemoveWhenEquiped.Cast<BaseMutation>().ToList());

                            mutationController.AddMutation(equipmentMutationApplicator.MutationsToApplyWhenEquiped.Cast<BaseMutation>().ToList(), teamId);
                            mutationController.AddImmunities(equipmentMutationApplicator.ImmunitiesToApplyWhenEquiped.Cast<BaseMutation>().ToList());
                        }
                    }
                }
            }
        }

        public void ListenerTriggered(FloatTrigger triggerResult)
        {
            if (triggerResult.value > 0)
            {
                SpawnEquipment();
            }
        }

        protected virtual void DespawnEquipment()
        {
            var teamComp = this.gameObject.GetComponentInParent<TeamComponent>();
            var teamId = teamComp == null ? 0 : teamComp.TeamIdentifier;

            // If there is a hand
            if (RightHand != null)
            {
                // If there is something in the hand.
                if (RightHand.transform.childCount == 1)
                {
                    // Get the default weapon
                    var rightHandEquipment = RightHand.transform.GetChild(0).gameObject;

                    if (rightHandEquipment != null && rightHandEquipment != this.gameObject)
                    {
                        // Remove any mutaitons from the equiper that are supposed to be removed.
                        var equipmentMutationApplicator = rightHandEquipment.GetComponent<EquipmentMutationApplicator>();
                        var mutationController = this.gameObject.GetComponentInParent<MutationController>();
                        if (equipmentMutationApplicator != null && mutationController != null)
                        {
                            mutationController.CureMutation(equipmentMutationApplicator.MutationsToCureWhenUnequiped.Cast<BaseMutation>().ToList());
                            mutationController.RemoveImmunities(equipmentMutationApplicator.ImmunitiesToCureWhenUnequiped.Cast<BaseMutation>().ToList());

                            mutationController.AddMutation(equipmentMutationApplicator.MutationsToApplyWhenUnequiped.Cast<BaseMutation>().ToList(), teamId);
                            mutationController.AddImmunities(equipmentMutationApplicator.ImmunitiesToApplyWhenUnequiped.Cast<BaseMutation>().ToList());
                        }

                        RightHand.transform.DetachChildren();
                        if (PhotonNetwork.inRoom)
                        {
                            try
                            {
                                if (this.GetComponent<PhotonView>().isMine)
                                {
                                    // perform network destroy
                                    GameManager3D.ConsoleMsg(string.Format("Network Destroy right hand equipment: {0}", rightHandEquipment.name));
                                    PhotonNetwork.Destroy(rightHandEquipment);
                                }
                                else
                                {
                                    GameManager3D.ConsoleMsg("Local (not mine) Destroy right hand equipment");
                                    Destroy(rightHandEquipment);
                                }
                            }
                            catch (System.Exception)
                            {
                                // Failed to network destroy, perform local destroy
                                GameManager3D.ConsoleMsg("Local (error) Destroy right hand equipment");
                                Destroy(rightHandEquipment);
                            }
                        }
                        else
                        {
                            // perform local destroy
                            GameManager3D.ConsoleMsg("Local (no-network) Destroy right hand equipment");
                            Destroy(rightHandEquipment);
                        }
                    }
                }
            }

            // If there is a hand
            if (LeftHand != null)
            {
                // If there is something in the hand.
                if (LeftHand.transform.childCount == 1)
                {
                    // Despawn the item
                    var leftHandEquipment = LeftHand.transform.GetChild(0).gameObject;

                    if (leftHandEquipment != null && leftHandEquipment != this.gameObject)
                    {
                        // Remove any mutaitons from the equiper that are supposed to be removed.
                        var equipmentMutationApplicator = leftHandEquipment.GetComponent<EquipmentMutationApplicator>();
                        var mutationController = this.gameObject.GetComponentInParent<MutationController>();
                        if (equipmentMutationApplicator != null && mutationController != null)
                        {
                            mutationController.CureMutation(equipmentMutationApplicator.MutationsToCureWhenUnequiped.Cast<BaseMutation>().ToList());
                            mutationController.RemoveImmunities(equipmentMutationApplicator.ImmunitiesToCureWhenUnequiped.Cast<BaseMutation>().ToList());

                            mutationController.AddMutation(equipmentMutationApplicator.MutationsToApplyWhenUnequiped.Cast<BaseMutation>().ToList(), teamId);
                            mutationController.AddImmunities(equipmentMutationApplicator.ImmunitiesToApplyWhenUnequiped.Cast<BaseMutation>().ToList());
                        }

                        LeftHand.transform.DetachChildren();
                        if (PhotonNetwork.inRoom)
                        {
                            try
                            {
                                if (this.GetComponent<PhotonView>().isMine)
                                {
                                    // perform network destroy
                                    GameManager3D.ConsoleMsg(string.Format("Network Destroy left hand equipment: {0}", leftHandEquipment.name));
                                    PhotonNetwork.Destroy(leftHandEquipment);
                                }
                                else
                                {
                                    GameManager3D.ConsoleMsg("Local (not mine) Destroy left hand equipment");
                                    Destroy(leftHandEquipment);
                                }
                            }
                            catch (System.Exception)
                            {
                                // Failed to network destroy, perform local destroy
                                GameManager3D.ConsoleMsg("Local (error) Destroy left hand equipment");
                                Destroy(leftHandEquipment);
                            }
                        }
                        else
                        {
                            // perform local destroy
                            GameManager3D.ConsoleMsg("Local (no-network) Destroy left hand equipment");
                            Destroy(leftHandEquipment);
                        }
                    }
                }
            }
        }

        protected void ProcessEquipment(bool disabled, GameObject gameObject, KeyCode toggleCode, string actionButton)
        {
            if (!disabled)
            {
                var equipment = gameObject.GetComponentInChildren<GameObjects.Weapon>();
                if (equipment != null)
                {
                    // determine if we are toggling the weapons activity
                    var toggleEquipment = Input.GetKeyDown(toggleCode);
                    if (toggleEquipment)
                    {
                        equipment.CycleFireMode();
                    }

                    // determine if we are firing the weapon
                    var fireButtonActive = equipment.GetFireMode() != FireMode.Beam ? Input.GetButtonDown(actionButton) : Input.GetButton(actionButton);
                    var isFiring = fireButtonActive;
                    var isRunning = false;

                    // Get the movement component;
                    var movementComponent = this.GetComponent<MovementComponent>();

                    // If there is a movement component.
                    if (movementComponent != null)
                    {
                        // Determine if we are running forward (not allowed to fire when running forward.
                        isRunning = movementComponent.IsRunningForward;
                        isFiring = isFiring && !movementComponent.IsRunningForward && !movementComponent.IsDucking;
                    }

                    if (equipment.GetFireMode() != FireMode.Beam)
                    {
                        // if we are firing.
                        if (isFiring)
                        {
                            // Active the current equipment.
                            equipment.Fire(GetComponentInParent<PlayerController3D>(), isRunning, Time.deltaTime);
                        }
                    }
                    else
                    {
                        // Active the current equipment.
                        equipment.FireBeam(GetComponentInParent<PlayerController3D>(), isRunning, Time.deltaTime, isFiring);
                    }
                }
            }
        }

        protected SpawnData GenereateSpawnData(GameObject subParentObject)
        {
            int? parentId = null;
            if (PhotonNetwork.inRoom)
            {
                if (this.photonView != null)
                {
                    parentId = this.photonView.viewID; 
                }
            }
            else
            {
                parentId = this.transform.GetInstanceID();
            }

            return new SpawnData
            {
                ParentId = parentId,
                SubParentName = subParentObject.name
            };
        }
    }
}