using Assets.Scripts;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class Pickup3D : Photon.MonoBehaviour, IPickup
{
    public Equipment Equipment;

    public int RespawnTimer = 2;

    public void OnTriggerEnter(Collider coll)
    {
        // Get the colliding items controller.
        var player = coll.transform.root.transform.GetComponent<PlayerController3D>();

        // If it was a palyer.
        if (player != null)
        {
            // If there is equiptment 
            if (Equipment != null)
            {
                if (Equipment.CanEquip(player))
                {
                    // Stop us triggering the collider more than once.
                    GetComponent<CapsuleCollider>().enabled = false;

                    // Destroy the current object.
                    if (PhotonNetwork.inRoom)
                    {
                        this.photonView.RPC("DestroyMe", PhotonNetworkSettings.DefaultRPCNetworkTarget);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }

                    // Get it to then repsawn using the respawn manager.
                    if (!string.IsNullOrEmpty(Equipment.GetPickupPrefabType()))
                    {
                        var pickupable = Resources.Load(Equipment.GetPickupPrefabType()) as GameObject;
                        if (pickupable != null)
                        {
                            // Equip it.
                            Equipment.Equip(player);

                            PickupManager3D.RegisterRespawn(new RespawnRequest
                            {
                                RespawnLocation = this.transform.position,
                                PrefabToRespawn = pickupable,
                                RespawnTimeInSeconds = RespawnTimer
                            });
                        }
                    }
                }
            }
        }
    }

    [PunRPC]
    public void DestroyMe()
    {
        Destroy(this.gameObject);
    }

    public void SetEquipment(IEquipment equipment)
    {
        Equipment = (Equipment)equipment;
    }
}


