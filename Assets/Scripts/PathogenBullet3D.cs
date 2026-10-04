using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects.Pathogens;
using System;
using UnityEngine;

public class PathogenBullet3D : BaseBullet<PathogenBulletData>, IPathogenBullet
{
    public GameObject PathogenCloudToSpawn;

    public ScriptableObject Pathogen;

    public void SetPathogen(ScriptableObject pathogen)
    {
        Pathogen = pathogen;
    }

    public override void Start()
    {
        base.Start();

        var castSpawnData = SpawnData as PathogenBulletData;
        if (castSpawnData != null)
        {
            SetPathogen(Resources.Load(castSpawnData.PathogenName) as ScriptableObject);
        }
    }

    public override void OnCollisionEnter(Collision collision)
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

            // cast the pathogen SCO as a pathogen.
            var castPathogen = Pathogen as Pathogen;

            // If we have a pathogen to apply and the bullet is not spawning a pathogen cloud.
            if (castPathogen != null && PathogenCloudToSpawn == null)
            {
                // Get the mutation controller from the collision target.
                var targetMutationController = collision.collider.GetComponentInParent<MutationController>();
                targetMutationController = targetMutationController ?? collision.transform.GetComponent<MutationController>();

                // If a mutation controller is found then 
                if (targetMutationController != null)
                {
                    // If the shooterId != the Player or or the bullet is flagged as an EnemyBullet.
                    if (PhotonNetwork.inRoom)
                    {
                        // If I own what has been hit
                        var targetPhotonView = collision.transform.GetComponent<PhotonView>();
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

        // Remove the bullet prefab.
        RemoveBullet(0, collision);
    }

    protected override void SpawnContactExplosion(Collision collision)
    {
        try
        {
            base.SpawnContactExplosion(collision);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }

        // If we have a pathogen cloud to spawn and the lifetime has passed or a collision is detected.
        if (this.PathogenCloudToSpawn != null && (Time.time - bulletAwakeTime > ((BulletData)SpawnData).BulletLifetime || collision != null))
        {
            // Get the contact point from the collision or the current bullet position.
            var spawnLoc = collision != null ? collision.contacts[0].point : this.transform.position;

            // Set the Y co-ord at 0
            spawnLoc.y = 0.05f;

            // Instantiate the cloud
            var pathogenCloud = Instantiate(this.PathogenCloudToSpawn, spawnLoc, Quaternion.identity);

            // If there is a pathogen
            if (Pathogen != null)
            {
                // Get the pathogen applicator
                var pathogenApplicator = pathogenCloud.GetComponent<PathogenApplicator>();
                if (pathogenApplicator != null)
                {
                    // Add the pathogen to the pathogen applicator
                    pathogenApplicator.Pathogen = Pathogen;
                    pathogenApplicator.TeamId = ((BulletData)SpawnData).ShooterId;
                }
            }

            // Rotate it by 90 deg.
            pathogenCloud.transform.eulerAngles = new Vector3(-90, 0, 0);
        }
        else
        {
            Debug.Log("No contact effect found.");
        }
    }
}
