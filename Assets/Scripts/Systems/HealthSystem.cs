using Assets.Scripts.Components;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    public class HealthSystem : ComponentSystem
    {
        protected struct Filter
        {
            public Transform Transform;
            public HealthComponent HealthComponent;
        }

        protected override void OnUpdate()
        {
            var entities = GetEntities<Filter>();
            for(int i = 0; i < entities.Length; i++)
            {
                HealthComponent healthComponent = null;

                try
                {
                    healthComponent = entities[i].HealthComponent;
                }
                catch (System.InvalidOperationException)
                {
                    // Swallow: This is due to the fact that the item has be dealloc from the filter while iterating.
                }
                
                if (healthComponent != null)
                {
                    if (healthComponent.CurrentValue <= 0)
                    {
                        TriggerDeath(entities[i]);
                    }
                }
            }
        }

        protected virtual void TriggerDeath(Filter entity)
        {
            var healthComponent = entity.HealthComponent;

            // Check if the death event is not null
            if (entity.HealthComponent.DeathEvent != null)
            {
                // Invoke the death event
                entity.HealthComponent.DeathEvent.Invoke();
            }
        }
    }
}