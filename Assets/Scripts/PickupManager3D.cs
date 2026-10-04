using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class RespawnRequest
    {
        public GameObject PrefabToRespawn = null;

        public Vector3 RespawnLocation = Vector3.zero;

        public Quaternion RespawnRotation = Quaternion.identity;

        public int RespawnTimeInSeconds = 0;
    }

    public class PickupManager3D : Photon.MonoBehaviour
    {
        public GameObject ItemContainer;

        private static PickupManager3D PickupManager;

        public static PickupManager3D instance
        {
            get
            {
                if (!PickupManager)
                {
                    PickupManager = FindObjectOfType<PickupManager3D>() as PickupManager3D;

                    if (!PickupManager)
                    {
                        Debug.LogError("There is no PickupManager found in the scene");
                    }
                }

                return PickupManager;
            }
        }

        public List<KeyValuePair<DateTime, RespawnRequest>> RespawnQueue = new List<KeyValuePair<DateTime, RespawnRequest>>();

        public static void RegisterRespawn(RespawnRequest respawnRequest)
        {
            GameManager3D.ConsoleMsg(string.Format("Registering respawn of {0} in {1} seconds.", respawnRequest.PrefabToRespawn.name, respawnRequest.RespawnTimeInSeconds));
            instance.RespawnQueue.Add(new KeyValuePair<DateTime, RespawnRequest>(DateTime.Now, respawnRequest));
        }

        public void Update()
        {
            var spawnedItems = new List<KeyValuePair<DateTime, RespawnRequest>>(); 

            // Get all the items thare are due for respawn.
            foreach (var respawn in RespawnQueue.Where(x => (DateTime.Now - x.Key).TotalSeconds >= x.Value.RespawnTimeInSeconds))
            {
                var respawnReq = respawn.Value;
                var respawnedItem = PhotonNetwork.inRoom
                    ? PhotonNetwork.Instantiate(respawnReq.PrefabToRespawn.name, respawnReq.RespawnLocation, respawnReq.RespawnRotation, 0)
                    : Instantiate(respawnReq.PrefabToRespawn, respawnReq.RespawnLocation, respawnReq.RespawnRotation);

                if (respawnedItem != null && ItemContainer != null)
                {
                    respawnedItem.transform.parent = ItemContainer.transform;
                }

                spawnedItems.Add(respawn);
            }

            // Remove completed resawpn requests.
            foreach (var respawnRequest in spawnedItems)
            {
                RespawnQueue.Remove(respawnRequest);
            }
        }
    }
}