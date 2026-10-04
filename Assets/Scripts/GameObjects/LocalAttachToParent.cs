using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class LocalAttachToParent : MonoBehaviour, IAttachToParent
{
    private SpawnData spawnData;

    public void Instantiate(object[] data)
    {
        if (data != null && data.Length == 1)
        {
            spawnData = JsonConvert.DeserializeObject<SpawnData>((string)data[0]);

            if (spawnData.ParentId.HasValue)
            {
                var parentGameObject = GameObject.FindObjectsOfType<Transform>().FirstOrDefault(x => x.gameObject.GetInstanceID() == spawnData.ParentId.Value);
                if (parentGameObject != null)
                {
                    var subParent = parentGameObject.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.gameObject.name == spawnData.SubParentName);
                    if (subParent != null)
                    {
                        transform.parent = subParent.transform;

                        // if we are parenting to a sub parent, then reset the position and rotation of the item relative to the sub-parent.
                        transform.localPosition = Vector3.zero;
                        transform.localRotation = Quaternion.identity;
                    }
                    else
                    {
                        transform.parent = parentGameObject.transform;
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("Failed to find gameobject with ID '{0}' on '{1}'", spawnData.ParentId, this.gameObject.name));
                }
            }
        }
    }
}