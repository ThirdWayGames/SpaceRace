using Assets.Scripts.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Assets.Scripts.Systems
{
    [UpdateAfter(typeof(InputSystem))]
    [UpdateAfter(typeof(FixedUpdate))]
    public class MovementSystem : ComponentSystem
    {
        protected struct Filter
        {
            public Transform Transform;
            public Rigidbody Rigidbody;
            public MovementComponent MovementComponent;
            public StaminaComponent StaminaComponent;
        }

        protected override void OnUpdate()
        {
            foreach (var entity in GetEntities<Filter>())
            {
                if (!entity.MovementComponent.DisableMovement)
                {
                    // Are we tranforming the position.
                    var rb = entity.Rigidbody;
                    var verticalMovementIndex = entity.MovementComponent.Vertical;
                    var horizontalMovementIndex = entity.MovementComponent.Horizontal;

                    if (rb != null)
                    {
                        // Get the current speed (this has to be done before removing gravitional vel otherwise it messes with the 
                        // forward vel calculation in the GetCurretSpeed process that determines if we are moving forward or backward.
                        entity.MovementComponent.CurrentSpeed = GetCurrentSpeed(entity);

                        // Remove all non gavitaional velocity.
                        var currentVel = rb.velocity;
                        currentVel = new Vector3(0, currentVel.y, 0);
                        rb.velocity = currentVel;

                        if (entity.MovementComponent.RotateTowardDirection)
                        {
                            var verticalRot = 0f;
                            var horizontalRot = 0f; 

                            // If the vertial button is being pressed
                            if (horizontalMovementIndex != 0f)
                            {
                                // If the value is < 0
                                if (horizontalMovementIndex > 0f)
                                {
                                    horizontalRot = 90f;
                                }
                                else
                                {
                                    horizontalRot = 270f;
                                }
                            }

                            if (verticalMovementIndex != 0f)
                            {
                                if (verticalMovementIndex > 0f)
                                {
                                    verticalRot = 360f;
                                }
                                else
                                {
                                    verticalRot = 180f;
                                }
                            }
                            
                            if (horizontalMovementIndex != 0f && verticalMovementIndex != 0f)
                            {
                                var horizValuePos = horizontalMovementIndex > 0f;
                                var vertValuePos = verticalMovementIndex > 0f;

                                // if ver pos then between -90, 0 and 90;
                                if (vertValuePos)
                                {
                                    entity.MovementComponent.RotateTowardDirectionAngle = horizValuePos ? 45f : 315f;
                                }
                                else
                                {
                                    // if ver pos then between -90, 180 and 90;
                                    entity.MovementComponent.RotateTowardDirectionAngle = horizValuePos ? 135f : 225f;
                                }
                            }
                            else
                            {
                                entity.MovementComponent.RotateTowardDirectionAngle = verticalRot > horizontalRot ? verticalRot : horizontalRot;
                            }

                            if (horizontalMovementIndex != 0f || verticalMovementIndex != 0f)
                            {
                                var facingVector = new Vector3(0, entity.MovementComponent.RotateTowardDirectionAngle, 0);
                                entity.Transform.rotation = Quaternion.Lerp(entity.Transform.rotation, Quaternion.Euler(facingVector), entity.MovementComponent.RotationLerpSpeed);
                            }
                        }

                        if (ShouldTranslate(verticalMovementIndex, horizontalMovementIndex))
                        {
                            // Adjust the velocity of the transform/rigid body if there is one.
                            var movementVector = new Vector3(horizontalMovementIndex, 0, verticalMovementIndex);
                            var translation = movementVector.normalized * entity.MovementComponent.CurrentSpeed;
                            if (entity.MovementComponent.RelativeMovement)
                            {
                                rb.AddRelativeForce(translation, entity.MovementComponent.CurrentForceMode);
                            }
                            else
                            {
                                rb.AddForce(translation, entity.MovementComponent.CurrentForceMode);
                            }
                        }
                        else
                        {
                            // If there are current forces but no input detected.
                            if (rb.velocity != Vector3.zero && rb.angularVelocity != Vector3.zero)
                            {
                                // Stop all force.
                                rb.velocity = Vector3.zero;
                                rb.angularVelocity = Vector3.zero;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns true when vertical or horizontal movement input is present.
        /// </summary>
        public static bool ShouldTranslate(float vertical, float horizontal)
        {
            return vertical != 0f || horizontal != 0f;
        }

        /// <summary>
        /// Gets the current speed that the clone is moving at.
        /// </summary>
        /// <param name="entity">The filtered entity.</param>
        /// <returns>The current speed the clone should be moving at.</returns>
        protected virtual float GetCurrentSpeed(Filter entity)
        {
            if (entity.MovementComponent.IsDucking)
            {
                AdjustStamina(entity, false);
                return entity.MovementComponent.CrouchSpeed;
            }

            var applyStaminaLoss = false;
            float returnSpeed = entity.MovementComponent.CurrentValue; ;

            // Get the local direction of movement.
            var localForwardVelocity = Vector3.Dot(entity.Rigidbody.velocity, entity.Transform.forward);

            // If we are moving forward and we are running
            if (localForwardVelocity >= 1 && entity.MovementComponent.IsRunning)
            {
                // Recalc the DoM
                CalcDom(entity);

                // If we are facing forward(ish)
                if (entity.MovementComponent.DirectionOfMovement > 0.8f)
                {
                    applyStaminaLoss = true;
                    returnSpeed = entity.MovementComponent.CurrentValue + entity.MovementComponent.MaxValue;
                }
            }
            else
            {
                CalcDom(entity);
            }

            AdjustStamina(entity, applyStaminaLoss);
            return returnSpeed;
        }

        /// <summary>
        /// Calculates the Direction of Movement.
        /// </summary>
        /// <param name="entity">The entity</param>
        protected void CalcDom(Filter entity)
        {
            // Re-calc the direction of movement
            var dirOfMove = entity.Transform.InverseTransformDirection(entity.Rigidbody.velocity).normalized.z;

            // If an error in the calc has not occured.
            if (dirOfMove != 0)
            {
                // Set the DOM
                entity.MovementComponent.DirectionOfMovement = dirOfMove;
            }
        }

        /// <summary>
        /// Adjusts the stamina.
        /// </summary>
        protected virtual void AdjustStamina(Filter entity, bool applyStaminaLoss)
        {
            var staminaComponent = entity.StaminaComponent;

            // Adjust the stamina
            staminaComponent.CurrentValue += (applyStaminaLoss ? staminaComponent.StaminaLossRate : staminaComponent.StaminaRefreshRate) * Time.deltaTime;

            // Make sure it never goes above 100 or below 0
            staminaComponent.CurrentValue = staminaComponent.CurrentValue > staminaComponent.MaxValue ? staminaComponent.MaxValue : staminaComponent.CurrentValue;
            staminaComponent.CurrentValue = staminaComponent.CurrentValue < 0 ? 0 : staminaComponent.CurrentValue;
        }
    }
}