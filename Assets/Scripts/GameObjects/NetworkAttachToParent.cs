using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using Newtonsoft.Json;
using Photon;
using System.Linq;
using UnityEngine;

public class NetworkAttachToParent : PunBehaviour, IAttachToParent
{
    public SpawnData SpawnData;

    public void Instantiate(object[] data)
    {
        if (data != null && data.Length == 1)
        {
            SpawnData = JsonConvert.DeserializeObject<SpawnData>((string)data[0]);
        }
    }

    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = this.gameObject.GetPhotonView().instantiationData;
        Instantiate(data);
        base.OnPhotonInstantiate(info);
    }

    public virtual void Start()
    {
        Attach(SpawnData);
    }

    public void Attach(ISpawnData spawnData)
    {
        if (SpawnData == null)
        {
            var castData = spawnData as SpawnData;
            SpawnData = castData ?? SpawnData;
        }

        if (transform.parent == null && SpawnData != null)
        {
            if (SpawnData.ParentId.HasValue)
            {
                var parentPhotonView = GameObject.FindObjectsOfType<PhotonView>().FirstOrDefault(x => x.viewID == SpawnData.ParentId);
                if (parentPhotonView != null)
                {
                    var parent = parentPhotonView.gameObject;
                    if (parent != null)
                    {
                        var subParent = parent.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.gameObject.name == SpawnData.SubParentName);
                        if (subParent != null)
                        {
                            transform.parent = subParent.transform;

                            // if we are parenting to a sub parent, then reset the position and rotation of the item relative to the sub-parent.
                            transform.localPosition = Vector3.zero;
                            transform.localRotation = Quaternion.identity;
                        }
                        else
                        {
                            transform.parent = parent.transform;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("Failed to find photon view of ID '{0}' on '{1}'", SpawnData.ParentId, this.gameObject.name));
                }
            }
        }
    }
}