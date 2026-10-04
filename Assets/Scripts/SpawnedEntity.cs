using Assets.Scripts.Interfaces;
using Newtonsoft.Json;
using System;
using UnityEngine;

public abstract class SpawnedEntity<T> : Photon.PunBehaviour where T : ISpawnData
{
    public ISpawnData SpawnData;

    public virtual void Awake()
    {
        if (SpawnData == null)
        {
            SpawnData = Activator.CreateInstance<T>();
        }
    }

    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        try
        {
            // Extract the bullet velocity/lifetime data from the message info post instantiate
            object[] data = this.gameObject.GetPhotonView().instantiationData;
            if (data != null && data.Length == 1)
            {
                SpawnData = JsonConvert.DeserializeObject<T>((string)data[0]);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }

        base.OnPhotonInstantiate(info);
    }
}
