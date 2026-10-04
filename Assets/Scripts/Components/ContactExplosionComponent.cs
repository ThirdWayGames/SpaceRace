using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Components
{
    [RequireComponent(typeof(GameObjectEntity))]
    public class ContactExplosionComponent : MonoBehaviour
    {
        public GameObject ContactExplosion;
    }
}