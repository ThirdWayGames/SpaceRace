using Assets.Scripts.Components;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    public class ContactExplosionSystem : ComponentSystem
    { 
        protected struct Filter
        {
            public Transform Transform;
            public ContactExplosionTriggerComponent ContactExplosionTrigger;
            public ContactExplosionComponent ContactExplosion;
        }

        protected override void OnUpdate()
        {
            foreach (var entity in GetEntities<Filter>())
            {
                var contactExplosionComponent = entity.ContactExplosion;
                var contactExplosionTrigger = entity.ContactExplosionTrigger;

                // If we have a contact explosion to instanciate.
                if (contactExplosionComponent.ContactExplosion != null)
                {
                    // If a player has been hit and I shot the bullet (Favour the shooter).
                    var player = entity.Transform.gameObject.GetComponentInParent<PlayerController3D>();
                    var playerHit = player != null;

                    Debug.Log(string.Format("Instantiate '{0}' particle system.", contactExplosionComponent.ContactExplosion.name));
                    if (contactExplosionTrigger != null)
                    {
                        var exp = GameObject.Instantiate(contactExplosionComponent.ContactExplosion, contactExplosionTrigger.ContactPoint, contactExplosionTrigger.ContactRotation);

                        if (playerHit)
                        {
                            // Get the particle renderer.
                            var contactParticleSystem = exp.GetComponent<ParticleSystem>();
                            var renderer = contactParticleSystem.GetComponent<Renderer>() as ParticleSystemRenderer;

                            if (renderer != null && renderer.trailMaterial != null)
                            {
                                // Turn on the trails.
                                var trails = contactParticleSystem.trails;
                                trails.enabled = true;
                            }
                        }
                    }

                    // Get the entity manager
                    var entityManager = World.Active.GetExistingManager<EntityManager>();

                    // Get the current entity.
                    var goEntityComponent = entity.Transform.gameObject.GetComponent<GameObjectEntity>();
                    var goEntity = goEntityComponent.Entity;

                    // Remove the component from the entity.
                    entityManager.RemoveComponent(goEntity, typeof(ContactExplosionTriggerComponent));
                    entityManager.Update();

                    if (contactExplosionTrigger != null)
                    {
                        GameObject.DestroyImmediate(contactExplosionTrigger);
                    }
                }
                else
                {
                    Debug.Log(string.Format("No contact effect found for {0}.", entity.Transform.gameObject.name));
                }
            }
        }
    }
}