using Assets.Scripts.Interfaces;
using System.Linq;
using UnityEngine;

public class EntitySpawnConsole : AccessConsole3D, IConsole
{
    public GameObject EntityToSpawn;

    public Vector3 SpawnLocation;

    public Vector3 SpawnRotation;

    public Vector3 SpawnScale;

    public bool AllowRespawn = false;

    public bool SpawnOnSuccess = false;

    public int RespawnCount = 0;

    public float RespawnTimer = 0f;

    public GameObject NetworkManager;

    protected int SpawnCount = 0;

    protected float NextSpawnTime = 0;

    protected GameObject SpawnedEntity;

    public override void Update()
    {
        base.Update();

        if (!SpawnOnSuccess)
        {
            SpawnEntity();
        }
    }

    public override void ConsoleComplete()
    {
        base.ConsoleComplete();

        // If we are in a network room.
        if (PhotonNetwork.inRoom && this.photonView != null)
        {
            if (SpawnOnSuccess)
            {
                // Despawn the spawned entity over the network.
                SpawnEntity();
            }
            else
            {
                // Despawn the spawned entity over the network.
                this.photonView.RPC("DestroyLocalEntity", PhotonNetworkSettings.EventTarget, null);
            }
        }
        else
        {
            if (SpawnOnSuccess)
            {
                SpawnEntity();
            }
            else
            {
                // Despawn the spawned entity locally
                DestroyLocalEntity();
            }
        }
    }

    [PunRPC]
    public virtual void DestroyLocalEntity()
    {
        // If we have a spawned entity.
        if (SpawnedEntity != null)
        {
            // Despawn the spawned entity locally
            Destroy(SpawnedEntity);

            // Make sure we set the spawned entity to null after destroying it.
            SpawnedEntity = null;

            // Set the next time the entity should be spawned after de-spawn
            NextSpawnTime = Time.time + RespawnTimer;
        }
    }

    protected void SpawnEntity()
    {
        // If we have an entity to spawn and we haven't spawned one yet.
        if (EntityToSpawn != null && SpawnedEntity == null)
        {
            /*If 
             * we have not spawned anyting yet (SpawnCount == 0)
             * or we are allowing respawns
             * AND
             * the last spawn time == zero (nothing spawned yet)
             * or the time now is >= than the lastspawn time + respawn timer
             * then spawn the entity.
            */
            if ((SpawnCount == 0 || AllowRespawn) && Time.time >= NextSpawnTime)
            {
                var locallySpawned = true;
                if (RespawnCount == 0 || SpawnCount <= RespawnCount)
                {
                    // If we are spawning on success
                    if (SpawnOnSuccess)
                    {
                        // And the prefab has a photon view
                        if (EntityToSpawn.GetComponent<PhotonView>() != null)
                        {
                            // Call a network spawn.
                            SpawnedEntity = PhotonNetwork.Instantiate(EntityToSpawn.name, SpawnLocation, Quaternion.Euler(SpawnRotation), 0);

                            // Transfer ownership of this object to the master client.
                            if (!PhotonNetwork.isMasterClient)
                            {
                                SpawnedEntity.GetPhotonView().TransferOwnership(PhotonNetwork.masterClient.ID);
                            } 

                            locallySpawned = false;
                        }
                        else
                        {
                            // Locally spawn the entity.
                            SpawnedEntity = Instantiate(EntityToSpawn, SpawnLocation, Quaternion.Euler(SpawnRotation));
                        }
                    }
                    else
                    { 
                        // Locally spawn the entity.
                        SpawnedEntity = Instantiate(EntityToSpawn, SpawnLocation, Quaternion.Euler(SpawnRotation));
                    }
                }

                // If the scale data is not zero and the item was locally spawned (only locally spawned items can have their scale adjusted)
                if (SpawnScale != Vector3.zero && locallySpawned)
                {
                    // Set the local scale.
                    SpawnedEntity.transform.localScale = SpawnScale;
                }

                // If we have spawned something 
                if (SpawnedEntity != null)
                {
                    SpawnCount += 1;

                }
            }
        }
    }
}
