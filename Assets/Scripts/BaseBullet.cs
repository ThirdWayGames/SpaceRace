using Assets.Scripts;
using Assets.Scripts.Components;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public abstract class BaseBullet<T> : SpawnedEntity<T>, IBullet where T : BulletData
{
    public List<ScriptableObject> MutationsToApply;

    public GameObject ContactExplosion;

    public bool OnlyDestroyOnCollision = true;

    public bool IsEnemyBullet = false;

    public float EnergyConsumption = -1f;

    protected int collisionCount = 0;

    protected float bulletAwakeTime;

    public override void Awake()
    {
        base.Awake();

        bulletAwakeTime = Time.time;

        // Make sure the mutation the bullet applies is a base mutation.
        if (MutationsToApply != null)
        {
            foreach (var MutationToApply in MutationsToApply)
            {
                var bulletMutation = MutationToApply as BaseMutation;

                // If the mutation is null (not castable as a base mutation)
                if (bulletMutation == null)
                {
                    // Throw an exception.
                    throw new Exception("MutationToApply on bullet is not of type 'BaseMutation'");
                }
            }
        }
    }

    public virtual void Start()
    {
        // Apply fore to the bullet based on the velocity and lifetime.
        if (SpawnData != null)
        {
            ApplyForce(SpawnData);
        }

        ShotEffects.Present(gameObject);
    }

    public virtual void ApplyForce(ISpawnData bulletData)
    {
        // Calculate the bullet velocity.
        var v = transform.forward * ((BulletData)bulletData).BulletVelocity;

        // Apply velocity to the bullets rigid body.
        this.GetComponentInChildren<Rigidbody>().velocity = v;

        // If we are not only destroying when we collide
        if (!OnlyDestroyOnCollision)
        {
            // Remove the bullet after the bullet lifetime.
            RemoveBullet(((BulletData)bulletData).BulletLifetime);
        }
    }

    public float GetEnergyConsumption()
    {
        return EnergyConsumption;
    }

    public virtual void RemoveBullet(float lifetime = 0f, Collision collision = null)
    {
        // Destroy the gameobject after the lifetime.
        Destroy(this.gameObject, lifetime);

        // Make sure we only spawn the contact explosion if it didn't hit us or is flagged as enemy bullet.
        SpawnContactExplosion(collision);
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collison hit");

        // If we have had more than one collision
        if (collisionCount > 0)
        {
            // Output to the console that we hit multiple objects.
            GameManager3D.ConsoleMsg("Multiple collisions detected, only first is registered.");
            return;
        }

        // Output a message to the console detailing who hit who.
        GameManager3D.ConsoleMsg(string.Format("{0}{3} registered collision [{1} collided with {2}]", PhotonNetwork.player.NickName, collision.contacts[0].thisCollider.name, collision.contacts[0].otherCollider.name, PhotonNetwork.isMasterClient ? ":[MC]" : string.Empty), false);

        // If I have not collided with anything yet.
        if (collisionCount == 0)
        {
            // Increment the collision count (this stops the bullet hitting multiple colliders on the player)
            collisionCount += 1;

            // If the bullet has a mutation to apply and the bullet has hit me.
            if (MutationsToApply != null)
            {
                // Get the mutation controller from the collision target.
                var targetMutationController = collision.collider.GetComponentInParent<MutationController>();
                targetMutationController = targetMutationController ?? collision.transform.GetComponent<MutationController>();

                // If a mutation controller is found then 
                if (targetMutationController != null)
                {
                    // Cast the MutationToApply as a BaseMutation (gurad statements in the Awake method)
                    var baseMutations = MutationsToApply.Cast<BaseMutation>().ToList();

                    // If the shooterId != the Player or or the bullet is flagged as an EnemyBullet.
                    if (PhotonNetwork.inRoom)
                    {
                        // If I own what has been hit (this stops dmg being applied universally by all clients and will only apply it if the client is responseible for the networked object)
                        var targetPhotonView = collision.transform.GetComponent<PhotonView>();
                        if (targetPhotonView != null)
                        {
                            if (targetPhotonView.isMine)
                            {
                                // Apply dmg.
                                GameManager3D.ConsoleMsg("AddMutation");
                                targetMutationController.AddMutation(baseMutations, ((BulletData)SpawnData).ShooterId);
                            }
                        }
                        else
                        {
                            // Apply dmg to the object (this is a local object in a network room. This means that all dmg is applied universally and each client is responsible
                            // for assigning dmg to local objects)
                            GameManager3D.ConsoleMsg("AddMutation");
                            targetMutationController.AddMutation(baseMutations, ((BulletData)SpawnData).ShooterId);
                        }
                    }
                    else
                    {
                        // Apply dmg.
                        targetMutationController.AddMutation(baseMutations, ((BulletData)SpawnData).ShooterId);
                    }
                }
            }
        }

        // Remove the bullet prefab.
        RemoveBullet(0, collision);
    }

    /// <summary>
    /// Spawns the contact explosion for the bullet.
    /// </summary>
    /// <param name="collision">The collision object</param>
    /// <param name="playerHit">Flag to determine if a player was hit by the bullet.</param>
    protected virtual void SpawnContactExplosion(Collision collision)
    {
        if (collision != null)
        { 
            // If the item collided with has its own ContactExplosionComponent
            var contactExplosionComponent = collision.collider.GetComponentInParent<ContactExplosionComponent>();
            if (contactExplosionComponent != null)
            {
                // Get the entity manager
                var entityManager = World.Active.GetExistingManager<EntityManager>();

                // Get the current entity.
                var goEntityComponent = collision.collider.GetComponentInParent<GameObjectEntity>();
                if (goEntityComponent != null)
                {
                    var entity = goEntityComponent.Entity;

                    if (entity != null && entityManager != null)
                    {
                        // Add a explosion trigger.
                        var contactExpTrigger = collision.collider.gameObject.AddComponent<ContactExplosionTriggerComponent>();

                        // Set the position and rotation of the explosion
                        var cp = collision.contacts[0];
                        contactExpTrigger.ContactPoint = cp.point;
                        contactExpTrigger.ContactRotation = Quaternion.FromToRotation(Vector3.forward, cp.normal);

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
                var player = collision.transform.GetComponentInParent<PlayerController3D>();
                var playerHit = player != null;

                // Spawn an explosion at the contact point.
                if (this.ContactExplosion != null)
                {
                    // Get the contact point.
                    var cp = collision.contacts[0];

                    // Get the rotation
                    var rot = Quaternion.FromToRotation(Vector3.forward, cp.normal);

                    Debug.Log(string.Format("Instantiate '{0}' particle system.", this.ContactExplosion.name));
                    var exp = Instantiate(this.ContactExplosion, cp.point, rot);

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

    public void SetShooterTeamId(int teamId)
    {
        if (SpawnData != null)
        {
            ((BulletData)SpawnData).ShooterId = teamId;
        }
    }

    public void SetSpawnData(ISpawnData spawnData)
    {
        SpawnData = spawnData;
    }
}
