using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Mutations/Zonal")]
[Serializable]
public class ZoneMutation : BaseMutation
{
    public GameObject ZoneToSpawn;

    private GameObject SpawnedZone;

    protected override void ExecutionTick(GameObject parentGameObject)
    {
        // Spawn the ZoneToSpawn as a child object of the main player.
        if (this.ZoneToSpawn != null)
        {
            SpawnedZone = Instantiate(this.ZoneToSpawn, parentGameObject.transform);
            SpawnedZone.transform.parent = parentGameObject.transform;

        }
        else
        {
            Debug.Log("No contact effect found.");
        }
    }

    public override void CureMutation(GameObject parentGameObject, GameObject gameObject)
    {
        base.CureMutation(parentGameObject, gameObject);

        if (isCured)
        {
            if (SpawnedZone != null)
            {
                Destroy(SpawnedZone);
            }
        }
    }
}

