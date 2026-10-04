using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components;
using Assets.Scripts.Controllers;
using Assets.Scripts.Enums;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts
{
    public abstract class BasePlayerController3D : BaseController, IPlayerController
    {
        /// <summary>
        /// Sets the maximum running speed.
        /// </summary>
        /// <param name="maxRunningSpeeed">The maximum running speeed.</param>
        public virtual void SetMaxRunningSpeed(float maxRunningSpeeed)
        {
        }

        public abstract void InitialiseClientSpawn();

        private EquipmentComponent EquipmentController;

        public void Start()
        {
            EquipmentController = this.GetComponent<EquipmentComponent>();
        }

        public void SetDisableActions(bool disabled)
        {
            if (EquipmentController != null)
            {
                EquipmentController.LeftHandDisabled = disabled;
                EquipmentController.RightHandDisabled = disabled;
            }
        }

        public bool IsActionsDisabled()
        {
            var result = false;
            if (EquipmentController != null)
            {
                result = EquipmentController.LeftHandDisabled && EquipmentController.RightHandDisabled;
            }

            return result;
        }

        public void SetDisableMovement(bool disabled)
        {
            var movementComponent = gameObject.GetComponent<MovementComponent>();
            if (movementComponent != null)
            {
                movementComponent.DisableMovement = disabled;
            }

            var mouseRotationComponent = gameObject.GetComponent<MouseRotationComponent>();
            if (mouseRotationComponent != null)
            {
                mouseRotationComponent.DisableRotation = disabled;
            }

            var rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody)
            {
                rigidbody.isKinematic = disabled;
            }
        }

        public int GetTeamId()
        {
            var result = 0;
            var teamComp = GetComponent<TeamComponent>();
            if (teamComp != null)
            {
                result = teamComp.TeamIdentifier;
            }

            return result;
        }

        public void SetPathogenLoadout(ScriptableObject pathogenLoadout)
        {
            var mutationController = GetComponent<MutationController>();
            if (mutationController != null)
            {
                mutationController.PersistentPathogen = pathogenLoadout;
            }
        }
    }
}