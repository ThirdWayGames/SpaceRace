using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Components
{
    [RequireComponent(typeof(PhotonView), typeof(GameObjectEntity))]
    public class MovementComponent : MutatableComponent
    {
        //// [HideInInspector]
        public float Horizontal;

        //// [HideInInspector]
        public float Vertical;

        //// [HideInInspector]
        public float CurrentSpeed;

        public bool RotateTowardDirection;
        public float RotateTowardDirectionAngle;
        public float RotationLerpSpeed = 10f;
        public bool DisableMovement;
        public bool RelativeMovement;
        public bool IsDucking = false;

        public float CrouchSpeedMultiplier = 0.5f;

        public bool IsRunning = false;
        public float DirectionOfMovement;
        public ForceMode CurrentForceMode;
        public string VerticalAxis = "Vertical";
        public string HorizontalAxis = "Horizontal";

        public bool IsMoving
        {
            get { return Horizontal != 0f || Vertical != 0f && !DisableMovement; }
            set { }
        }

        public bool IsRunningForward
        {
            get
            {
                return IsRunning && DirectionOfMovement > 0.8f;
            }
            set { }
        }

        public float CrouchSpeed
        {
            get
            {
                var multiplier = CrouchSpeedMultiplier < 0f ? 0f : CrouchSpeedMultiplier;
                return CurrentValue * multiplier;
            }
        }

        public override void Update()
        {
            // This has been overriden because the base method makes sure that the
            // CurrentValue is always less than the MaxValue (not something we want in this instance)
        }
    }
}