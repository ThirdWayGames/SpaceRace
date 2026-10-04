using Unity.Entities;
using UnityEngine;

public class EnemyPositionSystem : ComponentSystem
{

    public struct Group
    {
        public Transform Transform;
        public EnemyPositionComponent Position;
        public EnemyMovementComponent Movement;
    }

    protected override void OnUpdate()
    {
        // If master client or is not in room
        if (PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom)
        {
            foreach (var entity in GetEntities<Group>())
            {
                if (entity.Position.agent != null)
                {
                    Vector3 worldDeltaPosition = entity.Position.agent.nextPosition - entity.Transform.position;

                    //// Map 'worldDeltaPosition' to local space
                    //float dx = Vector3.Dot(entity.Transform.Right, worldDeltaPosition);
                    //float dy = Vector3.Dot(entity.Transform.forward, worldDeltaPosition);
                    //Vector2 deltaPosition = new Vector2(dx, dy);

                    //// Low-pass filter the deltaMove
                    //float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
                    //entity.Position.smoothDeltaPosition = Vector2.Lerp(entity.Position.smoothDeltaPosition, deltaPosition, smooth);

                    //// Update velocity if delta time is safe
                    //if (Time.deltaTime > 1e-5f)
                    //    entity.Position.velocity = entity.Position.smoothDeltaPosition / Time.deltaTime;

                    ////bool shouldMove = entity.Position.velocity.magnitude > 0.5f && entity.Position.agent.remainingDistance > entity.Position.agent.stoppingDistance;

                    //// Update animation parameters
                    //entity.Position.anim.SetBool("move", entity.Movement.IsMoving);
                    ////Debug.Log(velocity.magnitude);
                    //// anim.SetFloat ("Speed", velocity.magnitude);
                    //entity.Position.anim.SetFloat("velx", entity.Position.velocity.x);
                    //entity.Position.anim.SetFloat("vely", entity.Position.velocity.y);

                    if (worldDeltaPosition.magnitude > entity.Movement.Agent.radius)
                    {
                        entity.Transform.position = entity.Movement.Agent.nextPosition - 0.9f * worldDeltaPosition;
                    }
                }
            }
        }
    }
}
