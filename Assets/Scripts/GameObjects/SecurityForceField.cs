using System;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public class SecurityForceField : Photon.PunBehaviour
    {
        private Vector3 GameObjectScale = Vector3.zero;

        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            try
            {
                // Extract the bullet velocity/lifetime data from the message info post instantiate
                object[] data = this.gameObject.GetPhotonView().instantiationData;
                if (data != null)
                {
                    GameObjectScale.x = (float)data[0];
                    GameObjectScale.y = (float)data[1];
                    GameObjectScale.z = (float)data[2];
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }

            base.OnPhotonInstantiate(info);

            // Apply fore to the bullet based on the velocity and lifetime.
            if (GameObjectScale != Vector3.zero)
            {
                this.transform.localScale = GameObjectScale;
            }
        }
    }
}