using Assets.Scripts;
using Assets.Scripts.Components;
using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(PlayerPositionsSystem))]
public class EnemyTargetSystem : ComponentSystem
{
    public struct Group
    {
        public Transform transform;
        public PlayerPositionsComponent Players;
        public EnemyTargetComponent Target;
        public EnemyAttackComponent Attack;
        public EnemyStateComponent State;
        public EnemyTeamComponent Team;
    }

    protected override void OnUpdate()
    {
        // Make sure the master client is the one in controll of the AI systems.
        if (PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom)
        {
            // Loop the entities
            foreach (var entity in GetEntities<Group>())
            {
                // Check for hostile players (this will set a target if viable target avaiable)
                ProcessTargets(entity);
            }
        }
    }

    /// <summary>
    /// Checks for players within the vicinity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <returns>EnemyTargetComponent</returns>
    void ProcessTargets(Group entity)
    {
        // Create variable
        List<GameObject> targetsInViewRadius = new List<GameObject>();

        // Make sure there is a threat level for each player
        if (entity.Players.PlayerPositions.Any(x => x != null))
        {
            foreach (var playerPos in entity.Players.PlayerPositions.Where(x => x != null).ToList())
            {
                UpdateKnownThreatsWithPlayerPositions(entity, playerPos);
            }
        }

        // Determine all the threats that dont have a null target.
        if (entity.Target.KnownThreats.Any(x => x.Target != null))
        {
            foreach (var threat in entity.Target.KnownThreats.Where(x => x.Target != null))
            {
                ProcessThreatLevels(entity, threat);
            }
        }

        // Remove from known threats all the threats that I've not seen since my target cooldown (I lost them).
        var lostThreats = entity.Target.KnownThreats.Where(x => x.TimeLastSeen > entity.Target.targetCooldown).ToArray();
        for (int i = 0; i < lostThreats.Length; i++)
        {
            entity.Target.KnownThreats.Remove(lostThreats[i]);
        }

        // Remove from known threats that have no target (been killed?).
        var deadThreats = entity.Target.KnownThreats.Where(x => x.Target == null).ToArray();
        for (int i = 0; i < deadThreats.Length; i++)
        {
            entity.Target.KnownThreats.Remove(deadThreats[i]);
        }

        // Remove from known threats that are actually not hostile (on the same team).
        var friendlyThreats = entity.Target.KnownThreats.Where(x => !x.IsHostile).ToArray();
        for (int i = 0; i < friendlyThreats.Length; i++)
        {
            entity.Target.KnownThreats.Remove(friendlyThreats[i]);
        }

        // Providing have I have a known threat.
        var currentThreat = entity.Target.GetCurrentThreat();
        if (currentThreat != null)
        {
            // Set the target to the one with the highest threat.
            var hostileThreat = currentThreat;

            // Increment the time the target has been in my view.
            entity.Target.TimeTargetIsInView += Time.deltaTime;

            // Set the last know location.
            if (hostileThreat.IsInLos)
            {
                entity.Target.LastKnowLocation = entity.Target.GetCurrentThreat().Target.transform.position;
            }

            // If we are not already in an attack state and we have seen a target.
            if (entity.State.CurrentState != EnemyStateComponent.State.Attack && currentThreat.IsInLos)
            {
                // Play the alert clip.
                entity.Target.PlayAlertClip();
            }

            // Attak them.
            entity.State.CurrentState = EnemyStateComponent.State.Attack;
        }
        else
        {
            // If there are no known threats, then reset all target properties
            entity.Target.TimeTargetIsInView = 0;

            // And go back to patrolling
            entity.State.CurrentState = EnemyStateComponent.State.Patrol;
        }
    }

    protected bool TargetIsHostile(Group entity, GameObject target)
    {
        // Assume the target is hostile
        var targetIsHostile = true;

        // Get the player team component
        var teamComponent = target.GetComponent<TeamComponent>();

        // If the team component is not null
        if (teamComponent != null)
        {
            // Reassess the targets hostility towards me
            targetIsHostile = teamComponent.TeamIdentifier != entity.Team.TeamIdentifier;
        }

        return targetIsHostile;
    }

    /// <summary>
    /// Determines if the target is a ligitimate target for the entity based on;
    ///     if the target is within its look radius and view angle 
    ///     if it has line of sight
    ///     if its hostile.
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="target">The target.</param>
    /// <returns>True if all the conditions listed in the summary are true.</returns>
    protected bool AwareOfTarget(Group entity, GameObject target)
    {
        // Calc the distance to player
        var distanceToTarget = Vector3.Distance(entity.transform.position, target.transform.position);

        // Calc the direction to the player.
        var directionToTarget = (target.transform.position - entity.transform.position).normalized;

        // Is the player in my view angle?
        var inViewAngle = Vector3.Angle(entity.transform.rotation * new Vector3(0f, 0f, 1f), directionToTarget) < entity.Target.ViewAngle / 2;

        return inViewAngle;
    }

    protected void UpdateKnownThreatsWithPlayerPositions(Group entity, GameObject target)
    {
        // If we have a target and it is considered hostile.
        if (target != null && AwareOfTarget(entity, target) && TargetIsHostile(entity, target) && InLineOfSight(entity, target))
        {
            // Check to see if we know about it.
            var threat = entity.Target.KnownThreats.FirstOrDefault(x => x.Target == target);

            // If we dont.
            if (threat == null)
            {
                // Add it to the list of know threats.
                entity.Target.KnownThreats.Add(new ThreatLevel {Target = target, IsHostile = true });
            }
        }
    }

    /// <summary>
    /// Determines the threat level of the target and assigns it a score.
    /// </summary>
    /// <param name="target">The target</param>
    /// <returns>The threat level.</returns>
    protected ThreatLevel ProcessThreatLevels(Group entity, ThreatLevel threat)
    {
        if (threat.Target != null)
        {
            // Set the base threat level
            int threatLevel = 0;
            string threatScoreDesc = string.Empty;

            // If the target is facing me increase the threat level.
            float dot = Vector3.Dot(threat.Target.transform.forward, (entity.transform.position - threat.Target.transform.position).normalized);
            if (dot > 0.7f)
            {
                threatLevel += 1;
                threatScoreDesc += "F";
            }

            // If the target has weapons
            var targetEquipment = threat.Target.GetComponent<EquipmentComponent>();
            if (targetEquipment != null)
            {
                var weapon = targetEquipment.LeftHand.GetComponentInChildren<IWeaponary>();
                threatLevel += weapon != null ? 1 : 0;
                threatScoreDesc += "W";

                weapon = targetEquipment.RightHand.GetComponentInChildren<IWeaponary>();
                threatLevel += weapon != null ? 1 : 0;
                threatScoreDesc += "W";
            }

            // If the target is the current target maintain that threat.
            if (entity.Target.GetCurrentThreat() != null && threat.Target == entity.Target.GetCurrentThreat().Target)
            {
                threatLevel += 1;
                threatScoreDesc += "C";
            }

            // If the target is in LOS then increase the threat.
            var inLos = InLineOfSight(entity, threat.Target);
            if (inLos)
            {
                threatLevel += 1;
                threatScoreDesc += "L";
            }

            // If the target is too close ( < 75% of my effective range )
            if (Vector3.Distance(entity.transform.position, threat.Target.transform.position) < (entity.Attack.AttackDistance * .75))
            {
                // calculate the number we need to divide the distance by in order to increment the threatLevel (eg attack dist of 60 / 3 = segmentFactor of 20).
                var segmentFactor = (entity.Attack.AttackDistance * .75) / 3;

                // get the distance.
                var dist = Vector3.Distance(entity.transform.position, threat.Target.transform.position);

                // Calculate the threatAdjustment by dividing the distance by the segmentFactor (eg distance of 40 / segmentFactor of 20 = 2).
                var threatAdjustment = dist / segmentFactor;

                // Calulate the threatlevel by minusing the threat adjustment from the maximum proximity threat level (eg max prox threat of 3 - threat adjustment of 2 = 1)
                threatLevel += (int)(4 - threatAdjustment);
                threatScoreDesc += "P";

                for (int i = 0; i < threatAdjustment; i++)
                {
                    threatScoreDesc += "*";
                }
            }

            // Work out if I can shoot at the target or is something blocking me.
            var inShotLos = InShotLineOfSight(entity, threat.Target);

            // Update the stats.
            threat.IsInShotLos = inShotLos;
            threat.IsInLos = inLos;
            threat.IsHostile = TargetIsHostile(entity, threat.Target);
            threat.ThreatScoreDesc = threatScoreDesc;
            threat.Distance = Vector3.Distance(entity.transform.position, threat.Target.transform.position);
            threat.ThreatScore = threatLevel;

            // If the threat is not in LOS
            if (!threat.IsInLos)
            {
                // Increment its time last seen.
                threat.TimeLastSeen += Time.deltaTime;
            }
        }

        return threat;
    }

    protected bool InLineOfSight(Group entity, GameObject target)
    {
        // Get a layermask for the TransparentObstacles layer
        var layerMask = LayerMask.GetMask("TransparentObstacles");

        // Reverse it so that we are ignoreing raycast collisions on tht TransparentObstacles layer.
        layerMask = ~layerMask;

        // Assume we cant see the target.s
        var inLineOfSight = false;

        // Cast a ray at the target (ignoring collisions on TransparentObstacles layer).
        var raycastTransform = entity.Target.RayCastFrom != null ? entity.Target.RayCastFrom.transform : entity.transform;

        // If the raycast transform is higher than the entity base transform
        if (raycastTransform.transform.position.y > entity.transform.position.y)
        {
            // Calculate the 3 positions we need to cast from.
            var rayPos = new float[] {
                entity.Target.transform.position.y,
                (raycastTransform.transform.position.y - entity.transform.position.y) / 2,
                raycastTransform.transform.position.y
            };

            var tempTargetPos = target.transform.position;

            // Raycast 3 times.
            for (int i = 0; i < 3; i++)
            {
                // If we have not already got sight on the target.
                if (!inLineOfSight)
                {
                    // Adjust the height(y) co-ords of the target vector we are raycasting to.
                    tempTargetPos.y = rayPos[i];

                    // Calc the direction to the player.
                    var directionToTarget = (tempTargetPos - raycastTransform.position).normalized;

                    // Calc the distance to player
                    var distanceToTarget = Vector3.Distance(raycastTransform.position, tempTargetPos);

                    // Perform the raycast.
                    var rayHit = Physics.Raycast(raycastTransform.position, directionToTarget, out RaycastHit rayCastHitInfo, distanceToTarget, layerMask);
                    //// Debug.DrawLine(raycastTransform.position, tempTargetPos, Color.blue);

                    // Check to see that the ray hit a player.
                    inLineOfSight = rayHit && rayCastHitInfo.transform != null && rayCastHitInfo.transform.GetComponent<PlayerController3D>() != null;
                }
            }
        }
        else
        {
            // Calc the direction to the player.
            var directionToTarget = (target.transform.position - raycastTransform.position).normalized;

            // Calc the distance to player
            var distanceToTarget = Vector3.Distance(raycastTransform.position, target.transform.position);

            // Perform a raycast from the using the standard positions.
            var rayHit = Physics.Raycast(raycastTransform.position, directionToTarget, out RaycastHit rayCastHitInfo, distanceToTarget, layerMask);

            // Check to see that the ray hit a player.
            inLineOfSight = rayHit && rayCastHitInfo.transform != null && rayCastHitInfo.transform.GetComponent<PlayerController3D>() != null;
        }

        return inLineOfSight;
    }

    protected bool InShotLineOfSight(Group entity, GameObject target)
    {
        // Get a layermask for the Projectiles layer
        var layerMask = LayerMask.GetMask("Projectiles");

        // Reverse it so that we are ignoreing raycast collisions on tht Projectiles layer (stops bullets blocking shot LOS).
        layerMask = ~layerMask;

        // Assume we cant see the target.
        var inShotLineOfSight = false;

        var weapon = entity.Attack.GetWeapon() as Assets.Scripts.GameObjects.Weapon;
        if (weapon != null)
        {
            // Cast a ray at the target (ignoring collisions on TransparentObstacles layer).
            var gunPort = weapon.gameObject.transform.Find("Muzzle");
            var tempTargetPos = target.transform.position;

            // Adjust the height(y) co-ords of the target vector we are raycasting to.
            tempTargetPos.y = gunPort.transform.position.y;

            // Calc the direction to the player.
            var directionToTarget = (tempTargetPos - gunPort.position).normalized;

            // Calc the distance to player
            var distanceToTarget = Vector3.Distance(gunPort.position, tempTargetPos);

            // Perform the raycast.
            var rayHit = Physics.Raycast(gunPort.position, directionToTarget, out RaycastHit rayCastHitInfo, distanceToTarget, layerMask);

            // Check to see that the ray hit a player.
            if (rayHit)
            {
                var hitShield = rayCastHitInfo.collider != null && rayCastHitInfo.collider.gameObject.GetComponentInParent<ProjectileShieldComponent>() != null;
                var hitPlayer = rayCastHitInfo.transform != null && rayCastHitInfo.transform.GetComponent<PlayerController3D>() != null;
                inShotLineOfSight = hitPlayer && !hitShield;
            }

            if (inShotLineOfSight)
            {
                //// Debug.DrawLine(gunPort.position, tempTargetPos, Color.red);
            }
        }

        return inShotLineOfSight;
    }

    #region Unused Cover Code
    //EnemyTargetComponent CheckForCover(Group entity)
    //{
    //    List<GameObject> nearbyCover = new List<GameObject>();

    //    foreach (var cover in GameManager3D.Cover)
    //    {
    //        if (Vector3.Distance(cover.Transform.position, entity.Transform.position) < entity.Target.LookRadius)
    //        {
    //            nearbyCover.Add(cover);
    //        }
    //    }

    //    foreach (var cover in nearbyCover)
    //    {
    //        GameObject coverTarget = cover;
    //        Vector3 dirToTarget = (cover.Transform.position - entity.Transform.position).normalized;
    //        if (Vector3.Angle(entity.Transform.rotation * new Vector3(0f, 0f, 1f), dirToTarget) < entity.Target.ViewAngle / 2)
    //        {
    //            float distanceToPlayer = Vector3.Distance(entity.Transform.position, cover.Transform.position);
    //            if (!Physics.Raycast(entity.Transform.position, dirToTarget, distanceToPlayer, obstacleMask))
    //            {
    //                entity.Target.CoverAvailable = true;
    //                entity.Target.ClosestCover = cover;
    //            }
    //        }
    //    }


    //    return entity.Target;
    //}
    #endregion
}
