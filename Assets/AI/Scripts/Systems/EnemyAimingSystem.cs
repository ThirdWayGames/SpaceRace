using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(EnemyMovementSystem))]
public class EnemyAimingSystem : ComponentSystem
{
    public struct Group
    {
        public Transform Transform;
        public EnemyAimingComponent Aim;
        public EnemyTargetComponent Target;
    }

    protected override void OnUpdate()
    {
        foreach (var entity in GetEntities<Group>())
        {
            // If we have a target that is in view.
            if (entity.Target.GetCurrentThreat() != null && entity.Target.GetCurrentThreat().IsInLos)
            {
                // If we have a muzzle and a threat target.
                if (entity.Aim.Muzzle != null && entity.Target.GetCurrentThreat().Target != null)
                {
                    // Get the direction to the target from the muzzle.
                    var _direction = (entity.Target.GetCurrentThreat().Target.transform.position - new Vector3((entity.Aim.Muzzle.transform.position.x), entity.Aim.Muzzle.transform.position.y, entity.Aim.Muzzle.transform.position.z));

                    // Calculate the angle to the target.
                    entity.Aim.AngleToEnemy = Vector3.Angle(new Vector3(entity.Aim.Muzzle.transform.forward.x, 0, entity.Aim.Muzzle.transform.forward.z), new Vector3(_direction.x, 0, _direction.z));

                    // Calculate the right angle.
                    var right = Vector3.Angle(new Vector3(entity.Aim.Muzzle.transform.right.x, 0, entity.Aim.Muzzle.transform.right.z), new Vector3(_direction.x, 0, _direction.z));

                    // Check if angle is Left or Right
                    LeftOrRight leftOrRight = (right < 90 ? LeftOrRight.Right : LeftOrRight.Left);

                    // If angle is greater than 0 then increase or decrease the angle by 1
                    if (entity.Aim.AngleToEnemy > 2.5 || entity.Aim.AngleToEnemy < -2.5)
                    {
                        entity.Transform.rotation *= Quaternion.Euler(0, leftOrRight == LeftOrRight.Right ? 4 : -4, 0);
                    }
                }
            }
        }
    }
}
