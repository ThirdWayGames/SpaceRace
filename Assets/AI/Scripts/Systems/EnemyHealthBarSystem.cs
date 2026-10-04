using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class EnemyHealthBarSystem : ComponentSystem
{

    public struct Group
    {
        public EnemyStatsComponent Stats;
        public EnemyHealthBarComponent health;
    }


    protected override void OnUpdate()
    {
        foreach (var entity in GetEntities<Group>())
        {
            if (entity.Stats.isTakingDamage)
            {
                if (entity.health.ui != null)
                {
                    entity.health.ui.gameObject.SetActive(true);
                    entity.health.lastMadeVisibleTime = Time.time;
                    float healthPercent = (float) entity.Stats.curHealth/ entity.Stats.maxHealth;
                    Debug.Log("HEalth percent" + healthPercent);
                    entity.health.healthSlider.fillAmount = healthPercent;
                }

                entity.Stats.isTakingDamage = false;
            }

            if (entity.health.ui != null)
            {
                entity.health.ui.position = entity.health.target.position;
                entity.health.ui.forward = -entity.health.cam.forward;
                if (Time.time - entity.health.lastMadeVisibleTime > entity.health.visiableTime)
                {
                    entity.health.ui.gameObject.SetActive(false);
                }
            }
        }
    }
}
