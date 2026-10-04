using Assets.Scripts;
using Assets.Scripts.Components;
using Assets.Scripts.Interfaces;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(EnemyAimingSystem))]
public class EnemyAttackSystem : ComponentSystem
{
    public struct Group
    {
        public EnemyTargetComponent Target;
        public EnemyAttackComponent Attack;
        public EnemyStateComponent State;
        public EnemyAimingComponent Aim;
    }

    protected override void OnUpdate()
    {
        // Check if system is running on master client or is not in room
        if (PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom)
        {
            // Loop the entities
            foreach (var entity in GetEntities<Group>())
            {
                // If the current state is attack and we have a target that we can shoot at... Shoot HIM!
                if (entity.State.CurrentState == EnemyStateComponent.State.Attack)
                {
                    // If we have a current target that can be attacked with my weapon.
                    if (entity.Target.GetCurrentThreat() != null && entity.Target.GetCurrentThreat().CanAttack(entity.Attack.GetWeapon()))
                    {
                        // and the angle at which I am shooting them at is less than 80deg or I have a melee weapon (attack range < 2)
                        if (entity.Aim.AngleToEnemy < 80 || entity.Attack.AttackDistance < 2)
                        {
                            // Attack
                            Attack(entity.Attack);

                            // Is in combat
                            entity.Attack.InCombat = true;

                            // Set last attack time
                            entity.Attack.LastAttackTime = Time.time;
                        }
                    }

                    // Check if enemy has lost site of player for amount of time
                    if (Time.time - entity.Attack.LastAttackTime > entity.Attack.CombatCooldown)
                    {
                        // No longer in combat
                        entity.Attack.InCombat = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called to make the enemy attach
    /// </summary>
    /// <param name="attack"></param>
    void Attack(EnemyAttackComponent attack)
    {
        // Does the enemy have a weapon
        if (attack.GetWeapon() != null)
        {
            // Fire gun
            attack.GetWeapon().Fire(attack, false, Time.fixedDeltaTime, true);
        }
    }
}
