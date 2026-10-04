using System;
using UnityEngine;

namespace Assets.Scripts.OnPhotonInstantiate
{
    [CreateAssetMenu]
    [Serializable]
    public class OnPhotonInstantiateScriptableObject : ScriptableObject
    {
        [SerializeField]
        [ClassImplements(typeof(IOnPhotonInstantiate))]
        public ClassTypeReference OnPhotonNetworkInstantiateType;
    }
}
