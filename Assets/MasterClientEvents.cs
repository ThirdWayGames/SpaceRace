using System.Collections.Generic;
using UnityEngine;

public class MasterClientEvents : MonoBehaviour
{
    public List<GameObject> EnabledObjectsIfMasterClient;

    // Use this for initialization
    void Start ()
    {
        foreach (var gameObejectEnabledForMaster in EnabledObjectsIfMasterClient)
        {
            gameObejectEnabledForMaster.SetActive(PhotonNetwork.player.IsMasterClient);
        }
    }
}
