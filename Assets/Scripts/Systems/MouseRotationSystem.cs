using Assets.Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    public class MouseRotationSystem : ComponentSystem
    {
        public struct Filter
        {
            public Transform Transform;
            public MouseRotationComponent MouseRotationComponent;
        }

        protected override void OnUpdate()
        {
            foreach (var entity in GetEntities<Filter>())
            {
                // If we are in follow mouse mode.
                if (!entity.MouseRotationComponent.DisableRotation)
                {
                    if (entity.MouseRotationComponent.FollowMouse)
                    {
                        // Adjust the rotation 
                        entity.Transform.rotation = Quaternion.Lerp(entity.Transform.rotation, Quaternion.Euler(new Vector3(0, entity.MouseRotationComponent.GetMouseAngle(), 0)), entity.MouseRotationComponent.RotationSpeed);
                    }
                    else
                    {
                        // Else we are in mouse rotate played mode, which will make the camera rotate around the player too.
                        entity.Transform.Rotate(0, entity.MouseRotationComponent.RotationSpeed * entity.MouseRotationComponent.MouseHorizontal, 0);
                    }
                }
            }
        }
    }
}