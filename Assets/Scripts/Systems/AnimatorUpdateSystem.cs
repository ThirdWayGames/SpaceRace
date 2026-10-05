using Assets.Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    /// <summary>
    /// Updates the animator.
    /// </summary>
    [UpdateAfter(typeof(MovementSystem))]
    public class AnimatorUpdateSystem : ComponentSystem
    { 
        /// <summary>
        /// The filter used to determine which entities to execute this script on.
        /// </summary>
        protected struct Filter
        {
            public Transform Transform;
            public Rigidbody Rigidbody;
            public MovementComponent MovementComponent;
            public AnimationStateComponent AnimationStateComponent;
            public StaminaComponent StaminaComponent;
        }

        /// <summary>
        /// Picks the animator state. Standing still while crouched holds the duck pose.
        /// Moving while crouched plays the crouch walk.
        /// </summary>
        public static int ResolveState(bool isDucking, bool isMoving, bool isRunningForward)
        {
            if (isDucking)
            {
                return isMoving ? 4 : 3;
            }

            if (!isMoving)
            {
                return 0;
            }

            return isRunningForward ? 2 : 1;
        }

        /// <summary>
        /// Updates each entity that has been picked up by the filters.
        /// </summary>
        protected override void OnUpdate()
        {
            // For each entity.
            foreach (var entity in GetEntities<Filter>())
            {
                // Crouch idle stays on the duck pose. Crouch movement plays the crouch walk.
                var IsMoving = entity.MovementComponent.Vertical != 0f || entity.MovementComponent.Horizontal != 0f;
                var actualState = ResolveState(entity.MovementComponent.IsDucking, IsMoving, entity.MovementComponent.IsRunningForward);

                // Update the state.
                entity.AnimationStateComponent.SetState("State", actualState.ToString());
                entity.AnimationStateComponent.SetLayerWeight("LeftArmFire", entity.MovementComponent.IsRunningForward || entity.MovementComponent.IsDucking ? 0 : 1);
            }
        }
    }
}