using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    // Used to manage network state of the player.
    public class PlayerNetworkManager : Photon.MonoBehaviour
    {
        public bool PlayerNetworkReady = false;

        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(PlayerNetworkReady);
            }
            else
            {
                // Network player, receive data
                this.PlayerNetworkReady = (bool)stream.ReceiveNext();
            }
        }
    }
}