using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Mutations/Entity Spawn")]
[Serializable]
public class EntitySpawnMutation : TimedMutation
{
    [Header("Entity Spawn")]
    public GameObject PrefabToSpawn;

    [Header("Sub Parent")]
    [Tooltip("If you want the entity to attach to a game object below the parent in the hierarchy then specify its name here. ie. Muzzle")]
    public string SubParentName;

    public Vector3 SpawnPosition;

    public Vector3 SpawnRotation;

    public int EntitySpawnLimit = 10;

    protected bool CanSpawn = true;

    protected List<GameObject> SpawnedEntities;

    public override void Initialise(GameObject parentGameObject)
    {
        base.Initialise(parentGameObject);
        SpawnedEntities = new List<GameObject>();
    }

    protected override void ExecutionTick(GameObject parentGameObject)
    {
        base.ExecutionTick(parentGameObject);

        // Cleanse any dead entities from the list.
        for (int i = 0; i < SpawnedEntities.Count; i++)
        {
            if (SpawnedEntities[i] == null)
            {
                SpawnedEntities.Remove(SpawnedEntities[i]);
            }
        }

        // If we CanSpawn an entity and we have not reached our limit.
        if (CanSpawn && SpawnedEntities.Count + 1 <= EntitySpawnLimit)
        {
            GameObject spawnedObject = null;
            var spawnData = GenereateSpawnData().ToOjectArray();
            // If we are in a network room and both the target and the prefab have a photonview.
            if (PhotonNetwork.inRoom)
            {
                // Get the target spawn location
                var targetPhotonView = ParentGameObject.GetComponent<PhotonView>();
                var prefabPhotonView = PrefabToSpawn.GetComponent<PhotonView>();
                if (targetPhotonView != null && prefabPhotonView != null)
                {
                    spawnedObject = PhotonNetwork.Instantiate(PrefabToSpawn.name, GetTargetSpawnPosition() + SpawnPosition, GetTargetSpawnRotation() * Quaternion.Euler(SpawnRotation), 0, spawnData);
                }
                else
                {
                    Debug.LogWarning("Either Target or Spawn doesn't have a PhotonView and can not be spawned over the network.");
                }
            }
            else
            {
                spawnedObject = Instantiate(PrefabToSpawn, GetTargetSpawnPosition() + SpawnPosition, GetTargetSpawnRotation() * Quaternion.Euler(SpawnRotation));

                // Get the 
                LocalAttachToParent localAtp = null;
                var attachToParentComp = spawnedObject.GetComponent<IAttachToParent>();
                if (attachToParentComp != null)
                {
                    localAtp = spawnedObject.GetComponent<LocalAttachToParent>() ?? spawnedObject.AddComponent<LocalAttachToParent>();
                }

                if (localAtp != null)
                {
                    localAtp.Instantiate(spawnData);
                }
            }

            if (spawnedObject != null)
            {
                SpawnedEntities.Add(spawnedObject);
            }
        }
    }

    public virtual Vector3 GetTargetSpawnPosition()
    {
        return ParentGameObject.transform.localPosition;
    }

    public virtual Quaternion GetTargetSpawnRotation()
    {
        return ParentGameObject.transform.localRotation;
    }

    public SpawnData GenereateSpawnData()
    {
        int parentViewId = ParentGameObject.GetInstanceID();
        var photonView = ParentGameObject.GetComponent<PhotonView>();
        if (photonView != null && PhotonNetwork.inRoom)
        {
            parentViewId = photonView.viewID;
        }

        var result = new SpawnData {
            ParentId = parentViewId
        };

        if (!string.IsNullOrWhiteSpace(SubParentName))
        {
            result.SubParentName = SubParentName;
        }

        return result;
    }
}