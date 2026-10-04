using Assets.Scripts;
using Assets.Scripts.Components;
using Assets.Scripts.Enums;
using Assets.Scripts.GameObjects;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;
using static EnemyMovementComponent;

[UpdateAfter(typeof(EnemyTargetSystem))]
public class EnemyMovementSystem : ComponentSystem
{
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dist;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dist, float _angle)
        {
            hit = _hit;
            point = _point;
            dist = _dist;
            angle = _angle;
        }
    }

    public struct Group
    {
        public StaminaComponent Focus;
        public EnemyTargetComponent Target;
        public EnemyAttackComponent Attack;
        public EnemyMovementComponent Movement;
        public EnemyStateComponent State;
        public EnemyGuardComponent Guard;
    }

    protected override void OnUpdate()
    {
        // If master client or is not in room
        if (PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom)
        {
            // Loop the entities
            foreach (var entity in GetEntities<Group>())
            {
                // Calculate the attack distance (possibly should be done in a system of its own, but cant be deferred to the AttackSystem as this executes after the movement system
                // and we need to know the attack distance before we can determine if we can move.
                var weapon = entity.Attack.GetWeapon();
                var entityTransform = entity.Focus.transform.root;
                if (weapon != null)
                {
                    // Calc the attack distance (effective range * focusoffset% * 75%)
                    entity.Attack.AttackDistance = Mathf.Clamp(weapon.GetEffectiveRange() * (entity.Focus.CurrentValue / 100) * 0.75f, weapon.GetMinAttackRange(), weapon.GetEffectiveRange());
                }
                else
                {
                    Debug.LogWarning(string.Format("No weapon found on '{0}'", entity.Target.gameObject.name));
                }

                // Maintain the current destination
                var destinationVector = entity.Movement.CurrentDestination;

                // Get the current threat.
                var currentThreat = entity.Target.GetCurrentThreat();

                // Check if the current state is attack, we have a valid target and the Agent is on a nav mesh.
                if (entity.State.CurrentState == EnemyStateComponent.State.Attack && currentThreat != null)
                {
                    destinationVector = DetermineAttackPos(entity, currentThreat);
                }
                // Check if the state is patrol
                else if (entity.State.CurrentState == EnemyStateComponent.State.Patrol)
                {
                    // Check if the next waypoint is not null
                    if (entity.Movement.NextWaypoint != null)
                    {
                        // Set the destination
                        destinationVector = entity.Movement.NextWaypoint.transform.position;
                    }
                }
                // Check if the state is guard
                else if (entity.State.CurrentState == EnemyStateComponent.State.Guard)
                {
                    // Check if the guard position is not null
                    if (entity.Guard.GuardLocation != null)
                    {
                        // Calculate the distance to the guard location
                        if (Vector3.Distance(entityTransform.position, entity.Guard.GuardLocation.transform.position) > 0.2)
                        {
                            // Set the destination as the guard station
                            destinationVector = entity.Guard.GuardLocation.transform.position;

                            // Set in position
                            entity.Guard.InPosition = false;
                        }
                        else
                        {
                            // Rotate in position
                            RotateInGuardPosition(entity, entityTransform);
                        }
                    }
                }
                // Check if the state is wave
                else if (entity.State.CurrentState == EnemyStateComponent.State.Wave)
                {
                    // Set destination to target
                    destinationVector = currentThreat.Target.transform.position;
                }

                // check the minimum distance to current threat and re-calc defensive pos if required.
                if (entity.Movement.ApplyDefensiveMeasures)
                {
                    destinationVector = DetermineDefensivePos(entity, currentThreat, destinationVector);
                }

                // If the dest vector is different from my dest vector (change direction).
                if (destinationVector != entity.Movement.CurrentDestination)
                {
                    // If we are on a mesh
                    if (entity.Movement.Agent.isOnNavMesh)
                    {
                        // Set the current dest;
                        entity.Movement.CurrentDestination = destinationVector;

                        // Set the attack pos as destination
                        entity.Movement.Agent.SetDestination(destinationVector);

                        // If the velocity has become zero (for some reason)
                        if (entity.Movement.Agent.velocity == Vector3.zero)
                        {
                            // re-calc the velocity.
                            entity.Movement.Agent.velocity = entityTransform.forward * entity.Movement.Agent.speed;
                        }
                    }
                }

                // Calculate is moving from magnitude
                entity.Movement.IsMoving = entity.Movement.Agent.velocity.magnitude > 1;
            }
        }
    }

    /// <summary>
    /// Rotates the guard in position
    /// </summary>
    /// <param name="entity">The entity</param>
    void RotateInGuardPosition(Group entity, Transform entityTransform)
    {
        // Get the direction
        Vector3 direction = (entity.Guard.LookAtPos - entityTransform.position).normalized;

        // Get the look rotation
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Set the rotation
        entityTransform.rotation = Quaternion.Slerp(entityTransform.rotation, lookRotation, Time.deltaTime * 4f);
    }

    public Vector3 GetAltAttackPos(Group entity, Transform entityTransform, Vector3 posToAttack)
    {
        // Prepare a list of alt attack points.
        List<KeyValuePair<float, Vector3>> altAttackPoints = new List<KeyValuePair<float, Vector3>>();

        // This determines how many rays are sent out in a 360 degree circle.
        int stepCount = Mathf.RoundToInt(entity.Target.AltTargetScanRadius * .2f);

        // This calculates the angle of each step.
        float stepAngleSize = entity.Target.AltTargetScanRadius / stepCount;

        // Sets the last view cast 
        ViewCastInfo oldViewCast = new ViewCastInfo();

        // For each step in the current cycle of ray casting.
        for (int i = 0; i <= stepCount; i++)
        {
            // Get the current angle
            float angle = stepAngleSize * i;

            // Cast a ray out.
            ViewCastInfo viewCast = ViewCast(entity, entityTransform, angle);

            // Calculate the distance to the attack point.
            var attkPointDist = Vector3.Distance(entityTransform.position, viewCast.point);

            // If the attack point distance is between the min and max range of my weapon.
            if (attkPointDist > entity.Attack.GetWeapon().GetMinAttackRange() && attkPointDist < entity.Attack.GetWeapon().GetEffectiveRange())
            {
                // Add that to our view points
                altAttackPoints.Add(new KeyValuePair<float, Vector3>(attkPointDist, viewCast.point));
            }

            // Set the old (prev) cast to the current cast.
            oldViewCast = viewCast;
        }

        // Set the altAttackPoint as the last known loc,
        var altAttackPoint = entity.Target.LastKnowLocation;

        // if I have any alt attack points that are in range of my weapon.
        if (altAttackPoints.Any())
        {
            // Get the one that has the greatest distance from the target
            altAttackPoint = altAttackPoints.OrderByDescending(x => x.Key).First().Value;
        }

        // return that attack point to be my next location.
        return altAttackPoint;
    }

    /// <summary>
    /// Calculates a defensive position should the current threat be to close.
    /// </summary>
    /// <param name="entity">The entity group</param>
    /// <param name="currentThreat">the current threat</param>
    /// <param name="currentDestination">the current dest (used as a default return value)</param>
    /// <returns></returns>
    protected Vector3 DetermineDefensivePos(Group entity, ThreatLevel currentThreat, Vector3 currentDestination)
    {
        // Set the default destination to remain on target.
        var result = currentDestination;

        // if we have a current threat.
        if (currentThreat != null && currentThreat.Target != null)
        {
            // Get the entity transform
            var entityTransform = entity.Focus.transform.root;

            // If I can see the current threat
            if (currentThreat.IsInLos)
            {
                // Check to see if the distance between me and their current loc is > 75% of my effective range.
                if (Vector3.Distance(entityTransform.position, currentThreat.Target.transform.position) < (entity.Attack.AttackDistance * .75))
                {
                    result = GetAltAttackPos(entity, entityTransform, currentThreat.Target.transform.position);
                }
            }
            else
            {
                // Check to see if the distance between me and their last known loc is > 75% of my effective range.
                if (Vector3.Distance(entityTransform.position, entity.Target.LastKnowLocation) < (entity.Attack.AttackDistance * .75))
                {
                    result = GetAltAttackPos(entity, entityTransform, entity.Target.LastKnowLocation);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Determines the attack position I should use to effectivly attack the current threat.
    /// </summary>
    /// <param name="entity">The entity group</param>
    /// <param name="currentThreat">The current threat</param>
    /// <returns>The effective attack destination</returns>
    protected Vector3 DetermineAttackPos(Group entity, ThreatLevel currentThreat)
    {
        // Get the entity transform
        var entityTransform = entity.Focus.transform.root;

        if (currentThreat.IsInLos)
        {
            if (currentThreat.IsInShotLos)
            {
                if (currentThreat.Distance > entity.Attack.AttackDistance)
                {
                    /*
                     * I have LOS and SLOS on target but i am out of range.
                     * Move directly toward target.
                     */
                    return currentThreat.Target.transform.position;
                }
                else
                {
                    // Stop because I am now in range.
                    return entity.Movement.gameObject.transform.position;
                }
            }
            else
            {
                /*
                * I have LOS but no SLOS on target (range is irrelevant.
                * Move to a location where I can get SLOS on target.
                */
                return GetAltAttackPos(entity, entityTransform, currentThreat.Target.transform.position);
            }
        }
        else
        {
            return GetAltAttackPos(entity, entityTransform, entity.Target.LastKnowLocation);
        }
    }

    protected ViewCastInfo ViewCast(Group entity, Transform entityTransform, float globalAngle, bool findingEdge = false)
    {
        // Get the current weapon
        var weapon = entity.Attack.GetWeapon() as Assets.Scripts.GameObjects.Weapon;
        var posToCastFrom = entity.Target.GetCurrentThreat().Target.transform;

        // Use that weapon to determine what my current range that I want to raycast out to.
        var localViewDist = weapon != null ? weapon.GetEffectiveRange() : entity.Target.LookRadius;
        var gunShotLosHeight = weapon.gameObject.transform.Find("Muzzle").transform.position.y;

        // Calculate the direction from an angle
        var dir = DirFromAngle(entity, entityTransform, globalAngle, false);

        // Set the default return values
        var hitPoint = posToCastFrom.position + dir * localViewDist;
        var hitDistance = localViewDist;

        // Caclulate the angle at wich to cast the ray.
        var rayAngle = Vector3.Angle(dir, posToCastFrom.forward);

        // Cast out a rays
        var originVect = new Vector3(posToCastFrom.position.x, gunShotLosHeight, posToCastFrom.position.z);
        var adjustedDirVect = new Vector3(dir.x, 0, dir.z);
        var hit = Physics.Raycast(new Ray(originVect, adjustedDirVect), out RaycastHit rayHit, localViewDist);

        // If we hit something that wasn't the origin point.
        if (hit)
        {
            // Send back information based on what we hit.
            hitDistance = rayHit.distance;
            hitPoint = rayHit.point;
        }

        // Else send back info that will sayz` we basically hit the boundry of our view distance (in this instance the effective range of the current weapon)
        return new ViewCastInfo(false, hitPoint, hitDistance, globalAngle);
    }

    protected Vector3 DirFromAngle(Group entity, Transform entityTransform, float angleInDegrees, bool angleIsGlobal = true)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees -= entityTransform.eulerAngles.y;
        }

        return new Vector3(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Sin(angleInDegrees * Mathf.Deg2Rad));
    }
}
