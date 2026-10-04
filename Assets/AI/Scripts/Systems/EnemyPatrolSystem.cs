using System.Linq;
using Unity.Entities;
using UnityEngine;

public class EnemyPatrolSystem : ComponentSystem
{
    public struct Group
    {
        public Transform Transform;
        public EnemyMovementComponent Movement;
        public EnemyPatrolComponent Patrol;
    }

    protected override void OnUpdate()
    {
        // If master client or is not in room
        if (PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom)
        {
            foreach (var entity in GetEntities<Group>())
            {
                if (entity.Movement.NextWaypoint != null)
                {
                    entity.Movement.NextWaypoint = NextWaypoint(entity.Transform, entity.Movement);
                }
                else
                {
                    entity.Movement.NextWaypoint = CalculateNearestWaypoint(entity.Transform, entity.Movement);
                }
            }
        }
    }

    GameObject NextWaypoint(Transform transform, EnemyMovementComponent movement)
    {
        GameObject waypoint = null;

        if (movement.NextWaypoint != null)
        {
            waypoint = movement.NextWaypoint;
            if (Vector3.Distance(transform.position, waypoint.transform.position) < 4)
            {
                //movement.IsMoving = false;
                //var random = new System.Random();
                //movement.NextWaypoint = movement.waypoints[random.Next(0, movement.waypoints.Length)];

                if (movement.Waypoints != null && movement.Waypoints.Any())
                {
                    var index = movement.Waypoints.ToList().IndexOf(waypoint);
                    if (index + 1 < movement.Waypoints.Length)
                    {
                        waypoint = movement.Waypoints[index + 1];
                    }
                    else
                    {
                        waypoint = movement.Waypoints[0];
                    }
                }
            }
        }

        return waypoint;
    }

    GameObject CalculateNearestWaypoint(Transform transform, EnemyMovementComponent movement)
    {
        GameObject result = null;
        if (movement.Waypoints != null && movement.Waypoints.Any())
        {
            result = movement.Waypoints[0];
        }

        return result;

        /*int tMin = 0;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < movement.waypoints.Length; i++)
        {
            float dist = Vector3.Distance(movement.waypoints[i].transform.position, transform.position);
            if (dist < minDist)
            {
                tMin = i;
                minDist = dist;
            }
        }

        return movement.waypoints[tMin];*/
    }
}
