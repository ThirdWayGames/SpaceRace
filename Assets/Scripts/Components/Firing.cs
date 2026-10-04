using Unity.Entities;

namespace Assets.Scripts.Components
{
    public struct Firing : IComponentData
    {
        public float FiredAt;
    }

    public class FiringComponent : ComponentDataWrapper<Firing>
    {
    }
}