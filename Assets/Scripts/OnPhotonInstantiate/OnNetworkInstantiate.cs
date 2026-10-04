using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.OnPhotonInstantiate;
using Photon;
using UnityEngine;
using MonoBehaviour = UnityEngine.MonoBehaviour;

namespace Assets.AI.Scripts
{
    [System.Serializable]
    public class OnNetworkInstantiate : PunBehaviour
    {
        public List<OnPhotonInstantiateScriptableObject> OnPhotonInstantiateScriptableObjects;

        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            // Check if the types are not null
            if (OnPhotonInstantiateScriptableObjects != null)
            {
                // Check if we have any types in the list
                if (OnPhotonInstantiateScriptableObjects.Any())
                {
                    // Loop the types
                    foreach (var onPhotonInstantiateScript in OnPhotonInstantiateScriptableObjects)
                    {
                        // Check if class type reference is null
                        if (onPhotonInstantiateScript.OnPhotonNetworkInstantiateType != null)
                        {
                            // Check if the type within the type is not null
                            if (onPhotonInstantiateScript.OnPhotonNetworkInstantiateType.Type != null)
                            {
                                // Check if the type implemented IOnPhotonInstantiate and then create an instance
                                if (Activator.CreateInstance(onPhotonInstantiateScript.OnPhotonNetworkInstantiateType.Type) is IOnPhotonInstantiate instance)
                                {
                                    // Call on photon instantiate execute method
                                    instance.OnPhotonInstantiateExecute(this.gameObject.GetPhotonView().instantiationData, this.gameObject);
                                }
                            }
                        }
                    }
                }
            }

            base.OnPhotonInstantiate(info);
        }
    }
}
