using Unity.Rendering;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    public class BootStrap : MonoBehaviour
    {
        public static MeshInstanceRenderer BulletRenderer;

        [SerializeField]
        private MeshInstanceRenderer _bulletRenderer;

        private void Awake()
        {
            BulletRenderer = _bulletRenderer;
        }
    }
}