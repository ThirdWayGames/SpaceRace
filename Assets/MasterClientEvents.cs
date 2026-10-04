using System.Collections.Generic;
using UnityEngine;

public class MasterClientEvents : Photon.PunBehaviour
{
    public List<GameObject> EnabledObjectsIfMasterClient;

    void Start()
    {
        ApplyMasterObjects();
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        ApplyMasterObjects();
    }

    void ApplyMasterObjects()
    {
        if (EnabledObjectsIfMasterClient == null)
        {
            return;
        }

        var isMaster = PhotonNetwork.player != null && PhotonNetwork.player.IsMasterClient;
        foreach (var gameObejectEnabledForMaster in EnabledObjectsIfMasterClient)
        {
            if (gameObejectEnabledForMaster != null)
            {
                gameObejectEnabledForMaster.SetActive(isMaster);
            }
        }
    }
}
