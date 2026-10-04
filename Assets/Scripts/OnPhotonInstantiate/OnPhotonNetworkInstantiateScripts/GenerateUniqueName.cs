using System;
using UnityEngine;

namespace Assets.Scripts.OnPhotonInstantiate
{
    public class GenerateUniqueName : IOnPhotonInstantiate
    {
        /// <summary>
        /// Takes the existing name and makes it unique
        /// </summary>
        /// <param name="info">The instantiate info</param>
        public void OnPhotonInstantiateExecute(object[] data, GameObject gameObject)
        {
            // Null check the game object
            if (gameObject != null)
            {
                var photonView = gameObject.GetComponent<PhotonView>();
                if (photonView != null)
                {
                    // Create the new name
                    gameObject.transform.gameObject.name = string.Format("{0}_{1}", gameObject.transform.gameObject.name, photonView.viewID);
                }
            }
        }
    }
}
