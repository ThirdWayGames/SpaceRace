using Assets.Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    public class InputSystem : ComponentSystem
    {
        public struct Filter
        {
            public PhotonView PhotonView;
            public Transform Transform;
            public MovementComponent MovementComponent;
            public MouseRotationComponent RotationComponent;
        }

        protected override void OnUpdate()
        {
            foreach (var entity in GetEntities<Filter>())
            {
                var captureInputs = true;
                if (PhotonNetwork.inRoom)
                {
                    captureInputs = entity.PhotonView.isMine;
                }

                if (captureInputs)
                {
                    SetKeyboardInputs(entity);
                    SetMouseInputs(entity);
                }
            }
        }

        protected void SetKeyboardInputs(Filter entity)
        {
            if (!entity.MovementComponent.DisableMovement)
            {
                var shiftHeld = Input.GetKey(KeyCode.LeftShift);
                entity.MovementComponent.IsRunning = CameraFollow3D.AllowSprint(CameraFollow3D.LocalViewScoped(), shiftHeld);
                entity.MovementComponent.IsDucking = Input.GetKey(KeyCode.LeftControl) && !shiftHeld;

                var horizontal = Input.GetAxis(entity.MovementComponent.HorizontalAxis);
                var vertical = Input.GetAxis(entity.MovementComponent.VerticalAxis);
                if (entity.MovementComponent.IsDucking)
                {
                    // Ctrl is held, so A/S/D may be missing from the axis. Read the keys directly.
                    horizontal = MovementKeyState.ApplyKeys(
                        horizontal,
                        MovementKeyState.Held(KeyCode.A) || MovementKeyState.Held(KeyCode.LeftArrow),
                        MovementKeyState.Held(KeyCode.D) || MovementKeyState.Held(KeyCode.RightArrow));
                    vertical = MovementKeyState.ApplyKeys(
                        vertical,
                        MovementKeyState.Held(KeyCode.S) || MovementKeyState.Held(KeyCode.DownArrow),
                        MovementKeyState.Held(KeyCode.W) || MovementKeyState.Held(KeyCode.UpArrow));
                }

                entity.MovementComponent.Horizontal = horizontal;
                entity.MovementComponent.Vertical = vertical;
            }
        }

        protected void SetMouseInputs(Filter entity)
        {
            Vector3 mousePosition;
            Vector3 gameObjectWorldPos;

            if (!entity.RotationComponent.DisableRotation)
            {

                // Get the mouse position
                mousePosition = Input.mousePosition;

                // Get the current object position
                if (Camera.main != null)
                {
                    gameObjectWorldPos = Camera.main.WorldToScreenPoint(entity.Transform.position);

                    // calculate the mouse pos in relation to object positon.
                    mousePosition.x = mousePosition.x - gameObjectWorldPos.x;
                    mousePosition.y = mousePosition.y - gameObjectWorldPos.y;
                }

                // Calculate the amount of rotation required to make the object face toward the mouse pointer.
                // compensate by -90 deg so that the x axis points toward the target.
                entity.RotationComponent.Angle = -(Mathf.Atan2(mousePosition.y, mousePosition.x) * Mathf.Rad2Deg - 90);

                // If we are firing then we need to disable the rotate to direction and enable the mouse follow.
                entity.MovementComponent.RotateTowardDirection = entity.MovementComponent.IsRunning;
                entity.RotationComponent.FollowMouse = !entity.MovementComponent.IsRunning;
            }
        }
    }
}