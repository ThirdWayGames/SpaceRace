using System;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu]
    [Serializable]
    public class EquipmentLoadout : ScriptableObject
    {
        /// <summary>
        /// The Equipment to be spawned in the Left hand
        /// </summary>
        public GameObject LeftHandEquipment;

        /// <summary>
        /// The Equipment to be spawned in the Right hand
        /// </summary>
        public GameObject RightHandEquipment;
    }
}