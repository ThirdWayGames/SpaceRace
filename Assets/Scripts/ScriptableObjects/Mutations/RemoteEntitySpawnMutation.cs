using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Mutations/At Distance Entity Spawn")]
[Serializable]
public class RemoteEntitySpawnMutation : EntitySpawnMutation
{
    public float DistanceToSpawn;

    protected Vector3 TargetSpawnPosition;

    protected Quaternion TargetSpawnRotation;

    public override void Initialise(GameObject parentGameObject)
    {
        base.Initialise(parentGameObject);

        if (PrefabToSpawn.GetComponent<NetworkAttachToParent>() == null)
        {
            Debug.Log("Prefab to spawn requires AttachToParent component");
        }
    }

    protected override void ExecutionTick(GameObject parentGameObject)
    {
        // Calculate the TargetSpawnRoataion so that the spawned entity is always facing the player.
        // Calculate the TargetSpawnLocation based on 
        // 1. The direction the mutated entity is facing.
        // 2. The DistanceToSpawn
        // 3. Line of sight to the TargetSpawnLocation.
        TargetSpawnPosition = parentGameObject.transform.position + parentGameObject.transform.forward * DistanceToSpawn;

        // Adjust the height of the raycast positions that the rays will be performed above ground level.
        var originPos = parentGameObject.transform.position;
        var rayTarget = TargetSpawnPosition;

        originPos.y += .1f;
        rayTarget.y += .1f;

        // Cast a ray to the target location.
        RaycastHit hit;
        Physics.Linecast(originPos, rayTarget, out hit);

        // If we hit something.
        if (hit.collider != null && hit.collider.gameObject.transform.root != parentGameObject.transform.root)
        {
            // And the position of the collision is not the target location
            if (hit.collider.transform.position != TargetSpawnPosition)
            {
                // Move the spawn position closer to the mutated entity by 1 unit.
                var newDistance = Vector3.Distance(hit.collider.transform.position, parentGameObject.transform.position);
                TargetSpawnPosition = parentGameObject.transform.position + parentGameObject.transform.forward * (newDistance - 1f);
            }
        }

        // Set the rotation so that it faces the mutated entity.
        TargetSpawnRotation = Quaternion.LookRotation(parentGameObject.transform.position - TargetSpawnPosition);

        // call the base Execution so that it performs as normal.
        base.ExecutionTick(parentGameObject);
    }

    public override Vector3 GetTargetSpawnPosition()
    {
        return TargetSpawnPosition != Vector3.zero ? TargetSpawnPosition : ParentGameObject.transform.localPosition;
    }

    public override Quaternion GetTargetSpawnRotation()
    {
        return TargetSpawnRotation != Quaternion.identity ? TargetSpawnRotation : ParentGameObject.transform.localRotation;
    }
}