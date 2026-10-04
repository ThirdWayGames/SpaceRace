using System;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public class FlashLightController : MonoBehaviour
    {
        /// <summary>
        /// The flash light
        /// </summary>
        public Light FlashLight;

        /// <summary>
        /// The flash light
        /// </summary>
        public Light BackLight;

        public void Awake()
        {
            if (FlashLight == null)
            {
                throw new Exception("No Flashlight found.");
            }

            if (BackLight == null)
            {
                throw new Exception("No BackLight found.");
            }
        }

        // Update is called once per frame
        public void Update()
        {
            // If this is not my photon view
            if (!GetComponent<PhotonView>().isMine)
            {
                // Exit the method, this stop you turning the light of other clients on/off.
                return;
            }

            if (Input.GetKeyDown(KeyCode.F) && this.FlashLight != null)
            {
                // Turn the flashlight on.
                if (PhotonNetwork.inRoom)
                {
                    GetComponent<PhotonView>().RPC("ToggleFlashLight", PhotonNetworkSettings.EventTarget);
                }
                else
                {
                    ToggleFlashLight();
                }
            }
        }

        [PunRPC]
        public void ToggleFlashLight()
        {
            if (FlashLight != null)
            {
                this.FlashLight.enabled = !this.FlashLight.enabled;
            }

            if (BackLight != null)
            {
                this.BackLight.enabled = !this.BackLight.enabled;
            }
        }
    }
}