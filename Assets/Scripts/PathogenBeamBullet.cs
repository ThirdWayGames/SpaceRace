using Assets.Scripts;
using Assets.Scripts.Components;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects.Pathogens;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class PathogenBeamBullet : BaseBullet<PathogenBulletData>, IPathogenBullet
{
    [Header("Particle System used for Collision Detection")]
    public ParticleSystem part;

    public List<ParticleCollisionEvent> collisionEvents;

    protected Transform parentTrans;

    protected float bulletLifetime;

    public ScriptableObject Pathogen;

    public override void Start()
    {
        base.Start();

        var castSpawnData = SpawnData as PathogenBulletData;
        if (castSpawnData != null)
        {
            if (!string.IsNullOrWhiteSpace(castSpawnData.PathogenName))
            {
                SetPathogen(Resources.Load(castSpawnData.PathogenName) as ScriptableObject);
            }
        }

        if (part == null)
        {
            part = GetComponentInChildren<ParticleSystem>();
        }

        collisionEvents = new List<ParticleCollisionEvent>();

        AttachToParent();
    }

    public virtual void Update()
    {
        if (parentTrans != null)
        {
            this.transform.SetPositionAndRotation(parentTrans.position, parentTrans.rotation);
        }
    }

    public override void ApplyForce(ISpawnData bulletData)
    {
        // If we are not only destroying when we collide
        if (!OnlyDestroyOnCollision)
        {
            // Remove the bullet after the bullet lifetime.
            Destroy(this.gameObject, bulletLifetime);
        }
    }

    protected void AttachToParent()
    {
        if (transform.parent == null && SpawnData != null && ((SpawnData)SpawnData).ParentId.HasValue)
        {
            if (PhotonNetwork.inRoom)
            {
                var parentPhotonView = GameObject.FindObjectsOfType<PhotonView>().FirstOrDefault(x => x.viewID == ((SpawnData)SpawnData).ParentId);
                if (parentPhotonView != null)
                {
                    var parent = parentPhotonView.gameObject;
                    if (parent != null)
                    {
                        var subParent = parent.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.gameObject.name == ((SpawnData)SpawnData).SubParentName);
                        if (subParent != null)
                        {
                            transform.parent = subParent.transform;
                        }
                        else
                        {
                            transform.parent = parent.transform;
                        }

                        transform.localPosition = Vector3.zero;
                        var aimed = SpawnData as BulletData;
                        transform.localRotation = ShotAim.BeamLocalRotation(aimed != null ? aimed.AimYaw : 0f);
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("Failed to find photon view of ID '{0}' on '{1}'", ((SpawnData)SpawnData).ParentId, this.gameObject.name));
                }
            }
            else
            {
                var parentGameObject = GameObject.FindObjectsOfType<Transform>().FirstOrDefault(x => x.GetInstanceID() == ((SpawnData)SpawnData).ParentId.Value);
                if (parentGameObject != null)
                {
                    var subParent = parentGameObject.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.gameObject.name == ((SpawnData)SpawnData).SubParentName);
                    if (subParent != null)
                    {
                        transform.parent = subParent.transform;
                    }
                    else
                    {
                        transform.parent = parentGameObject.transform;
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("Failed to find gameobject with ID '{0}' on '{1}'", ((SpawnData)SpawnData).ParentId, this.gameObject.name));
                }
            }
        }
    }

    /// <summary>
    /// Spawns the contact explosion for the bullet.
    /// </summary>
    /// <param name="collision">The collision object</param>
    /// <param name="playerHit">Flag to determine if a player was hit by the bullet.</param>
    protected virtual void SpawnContactExplosion(ParticleCollisionEvent collision)
    {
        if (collision.colliderComponent != null)
        {
            // If the item collided with has its own ContactExplosionComponent
            var contactExplosionComponent = collision.colliderComponent.GetComponentInParent<ContactExplosionComponent>();
            if (contactExplosionComponent != null)
            {
                // Get the entity manager
                var entityManager = World.Active.GetExistingManager<EntityManager>();

                // Get the current entity.
                var goEntityComponent = collision.colliderComponent.GetComponentInParent<GameObjectEntity>();
                if (goEntityComponent != null)
                {
                    var entity = goEntityComponent.Entity;

                    if (entity != null && entityManager != null)
                    {
                        // Add a explosion trigger.
                        var contactExpTrigger = collision.colliderComponent.gameObject.AddComponent<ContactExplosionTriggerComponent>();

                        // Set the position and rotation of the explosion
                        contactExpTrigger.ContactPoint = collision.intersection;
                        contactExpTrigger.ContactRotation = Quaternion.LookRotation(collision.normal);

                        // Remove the component from the entity.
                        if (!entityManager.HasComponent(entity, typeof(ContactExplosionTriggerComponent)))
                        {
                            entityManager.AddComponent(entity, typeof(ContactExplosionTriggerComponent));
                            var setCompObjectMethod = typeof(EntityManager).GetMethod("SetComponentObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (setCompObjectMethod != null)
                            {
                                setCompObjectMethod.Invoke(entityManager, new object[] { entity, ComponentType.Create<ContactExplosionTriggerComponent>(), contactExpTrigger });
                            }

                            entityManager.Update();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("'{0}' has a ContactExplosionComponent but no GameObjectEntity so can not use entity systems.");
                    }
                }
            }
            else
            {
                // If a player has been hit and I shot the bullet (Favour the shooter).
                var player = collision.colliderComponent.GetComponentInParent<PlayerController3D>();
                var playerHit = player != null;

                // Spawn an explosion at the contact point.
                if (this.ContactExplosion != null)
                {
                    // Get the rotation
                    var rot = Quaternion.LookRotation(collision.normal);

                    Debug.Log(string.Format("Instantiate '{0}' particle system.", this.ContactExplosion.name));
                    var exp = Instantiate(this.ContactExplosion, collision.intersection, rot);

                    var contactParticleSystem = exp.GetComponent<ParticleSystem>();
                    if (playerHit && contactParticleSystem != null)
                    {
                        // Get the particle renderer.
                        var renderer = contactParticleSystem.GetComponent<Renderer>() as ParticleSystemRenderer;

                        if (renderer != null && renderer.trailMaterial != null)
                        {
                            // Turn on the trails.
                            var trails = contactParticleSystem.trails;
                            trails.enabled = true;
                        }
                    }
                }
                else
                {
                    Debug.Log("No contact effect found.");
                }
            }
        }
    }

    public void SetPathogen(ScriptableObject pathogen)
    {
        Pathogen = pathogen;
    }

    public virtual void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
        int i = 0;

        while (i < numCollisionEvents)
        {
            // If I have not collided with anything yet.
            if (collisionCount == 0)
            {
                // Increment the collision count (this stops the bullet hitting multiple colliders on the player)
                collisionCount += 1;

                // cast the pathogen SCO as a pathogen.
                var castPathogen = Pathogen as Pathogen;

                // If we have a pathogen to apply and the bullet is not spawning a pathogen cloud.
                if (castPathogen != null)
                {
                    // Get the mutation controller from the collision target.
                    var targetMutationController = other.GetComponentInParent<MutationController>();
                    targetMutationController = targetMutationController ?? other.transform.GetComponent<MutationController>();

                    // If a mutation controller is found then 
                    if (targetMutationController != null)
                    {
                        // If the shooterId != the Player or or the bullet is flagged as an EnemyBullet.
                        if (PhotonNetwork.inRoom)
                        {
                            // If I own what has been hit
                            var targetPhotonView = other.transform.GetComponent<PhotonView>();
                            if ((targetPhotonView != null && targetPhotonView.isMine))
                            {
                                // Apply dmg.
                                targetMutationController.AddPathogen(castPathogen, ((BulletData)SpawnData).ShooterId);
                            }
                        }
                        else
                        {
                            // Apply dmg.
                            targetMutationController.AddPathogen(castPathogen, ((BulletData)SpawnData).ShooterId);
                        }
                    }
                }
            }

            //// SpawnContactExplosion(collisionEvents[i]);

            i++;
        }
    }

    public void SetSpawnData(PathogenBulletData spawnData)
    {
        SpawnData = spawnData;
    }
}
