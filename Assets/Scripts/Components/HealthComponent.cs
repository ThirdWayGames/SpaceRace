using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class HealthComponent : MutatableComponent
    {
        private List<IDisposeCallback> DisposeCallbacks;

        public GameObject RagDollPrefab;

        public Action DeathEvent;

        private bool RagDollSpawned = false;

        public void Awake()
        {
            DisposeCallbacks = GetComponentsInChildren<IDisposeCallback>().ToList();
            DeathEvent += TriggerDeathEventCallBacks;
        }

        public void TriggerDeathEventCallBacks()
        {
            if (!OwnsDeathSideEffects())
            {
                return;
            }

            if (!RagDollSpawned)
            {
                // Instantiate the ragdoll.
                GameObject ragdoll = null;
                if (RagDollPrefab != null)
                {
                    if (PhotonNetwork.inRoom)
                    {
                        ragdoll = PhotonNetwork.Instantiate(RagDollPrefab.name, transform.position, Quaternion.identity, 0) as GameObject;
                    }
                    else
                    {
                        ragdoll = Instantiate(RagDollPrefab, transform.position, Quaternion.identity) as GameObject;
                    }
                }

                // If we have a ragdoll make sure its oriented the same as the main clone.
                if (ragdoll != null)
                {
                    RagDollSpawned = true;

                    // Get the bodies of both the main clone and the ragdoll
                    var ragdollBody = ragdoll.transform.Find("body");

                    ragdollBody = ragdollBody ?? ragdoll.transform.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name == "body");

                    var cloneBody = transform.Find("body");
                    cloneBody = cloneBody ?? transform.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name == "body");

                    if (ragdollBody != null && cloneBody != null)
                    {
                        Transform[] ragdollJoints = null;
                        Transform[] cloneJoints = null;

                        // If we have a ragdoll get the transforms
                        ragdollJoints = ragdollBody.GetComponentsInChildren<Transform>();

                        // If we have a main clone get the transforms
                        cloneJoints = cloneBody.GetComponentsInChildren<Transform>();

                        // If we have both the transforms for the main clone and the ragdoll
                        if (ragdollJoints != null && ragdollJoints.Length > 0 && cloneJoints != null && cloneJoints.Length > 0)
                        {
                            // for each rag doll joint.
                            foreach (var ragdollJoint in ragdollJoints)
                            {
                                // Find the associated join in the main clones joints
                                var foundcloneJoint = cloneJoints.FirstOrDefault(x => x.name == ragdollJoint.name);

                                // If we found an assoc. joint.
                                if (foundcloneJoint != null)
                                {
                                    // orient the rag doll join to the clone joint.
                                    ragdollJoint.position = foundcloneJoint.position;
                                    ragdollJoint.rotation = foundcloneJoint.rotation;
                                }
                            }
                        }
                    }
                }
            }

            // If we have any GOs to perform a dispose callback on.
            if (DisposeCallbacks != null && DisposeCallbacks.Any())
            {
                // For each GO that implements the IDisposeCallBack interface.
                foreach (var disposeCallBack in DisposeCallbacks)
                {
                    // Call the dispose item method.
                    disposeCallBack.DisposeItem(Time.deltaTime);
                }
            }

            // If the player controller is not null
            var playerController = GetComponentInParent<PlayerController3D>();
            if (playerController != null)
            {
                // Despawn the player.
                PlayerManager3D.Get().DespawnPlayer(playerController);
            }
            else
            {
                //  If we are in a network room
                if (PhotonNetwork.inRoom)
                {
                    // And this component is networked
                    if (this.GetComponent<PhotonView>() != null)
                    {
                        // Network destroy the object
                        PhotonNetwork.Destroy(transform.gameObject);
                    }
                    else
                    {
                        // Else locally destroy the game object.
                        Destroy(transform.gameObject);
                    }
                }
                else
                {
                    // Locally destroy the game object.
                    Destroy(transform.gameObject);
                }
            }
        }

        /// <summary>
        /// Takes an IOnCloneDeath script and runs it passing the current game object as a parameter
        /// </summary>
        /// <param name="onClonedDeathScript">Interface of script</param>
        public void TriggerOnDeathScript(IOnCloneDeath onClonedDeathScript)
        {
            // Check if a script is available
            if (onClonedDeathScript != null)
            {
                // Execute the script
                onClonedDeathScript.OnCloneDeath(gameObject);
            }
        }

        bool OwnsDeathSideEffects()
        {
            if (!PhotonNetwork.inRoom)
            {
                return true;
            }

            var view = GetComponent<PhotonView>();
            if (view == null)
            {
                view = GetComponentInParent<PhotonView>();
            }

            if (view == null)
            {
                return true;
            }

            return view.isMine;
        }
    }
}