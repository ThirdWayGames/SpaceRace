using System;
using Unity.Entities;
using UnityEngine;

public class EnemyStatsSystem : ComponentSystem
{
    public struct Group
    {
        public EnemyStatsComponent EnemyStats;
        public EnemyAnimationStateComponent Animation;
        public Transform Transform;
    }

    protected override void OnUpdate()
    {
        // If master client or is not in room
        if (PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom)
        {

            foreach (var entity in GetEntities<Group>())
            {
                if (entity.EnemyStats.damageToSubtract != 0)
                {
                    entity.EnemyStats.isTakingDamage = true;
                    entity.EnemyStats.curHealth = TakeDamage(entity);

                    entity.EnemyStats.damageToSubtract = 0;

                    if (entity.EnemyStats.curHealth <= 0)
                    {
                        OnDeath(entity);
                    }
                }
            }
        }
    }

    public float TakeDamage(Group entity)
    {
        //entity.Animation.Animator.Damage();
        entity.EnemyStats.damageToSubtract -= entity.EnemyStats.armor.GetValue();

        entity.EnemyStats.damageToSubtract = Mathf.Clamp(Math.Abs(entity.EnemyStats.damageToSubtract), 0, int.MaxValue);

        entity.EnemyStats.curHealth -= entity.EnemyStats.damageToSubtract;
        Debug.Log(entity.Transform.name + "Takes " + entity.EnemyStats.damageToSubtract + "Damage");

        return entity.EnemyStats.curHealth;
    }

    public void OnDeath(Group entity)
    {
        // Die
        Debug.Log("enemy dead");
        entity.Animation.Animator.Death();
        entity.EnemyStats.InvokeOnDeathEvent();
    }
}
