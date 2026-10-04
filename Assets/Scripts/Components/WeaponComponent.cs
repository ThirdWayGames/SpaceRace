using System;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Components
{
    [Serializable]
    public struct Weapon : IComponentData
    {
        public float BulletVelocity;
        public float BulletLifetime;
    }

    public class WeaponComponent : ComponentDataWrapper<Weapon>
    {
        public GameObject BulletPrefab;
    }
}