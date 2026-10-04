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
        /// Updates each entity that has been picked up by the filters.
        /// </summary>
        protected override void OnUpdate()
        {
            // For each entity.
            foreach (var entity in GetEntities<Filter>())
            {
                // Determine the state.
                var IsMoving = entity.MovementComponent.Vertical != 0f || entity.MovementComponent.Horizontal != 0f;
                var actualState = entity.MovementComponent.IsDucking ? 3 : IsMoving ? entity.MovementComponent.IsRunningForward ? 2 : 1 : 0;

                // Update the state.
                entity.AnimationStateComponent.SetState("State", actualState.ToString());
                entity.AnimationStateComponent.SetLayerWeight("LeftArmFire", entity.MovementComponent.IsRunningForward || entity.MovementComponent.IsDucking ? 0 : 1);
            }
        }
    }
}