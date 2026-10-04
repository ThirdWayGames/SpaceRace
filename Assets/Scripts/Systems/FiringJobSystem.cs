using Assets.Scripts.Components;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
    public class FiringJobSystem : JobComponentSystem
    {
        private ComponentGroup _componentGroup;

        protected override void OnCreateManager(int capacity)
        {
            _componentGroup = GetComponentGroup(
                ComponentType.Create<Firing>(),
                ComponentType.Create<Weapon>(),
                ComponentType.Create<Position>(),
                ComponentType.Create<Rotation>());

            _componentGroup.SetFilterChanged(ComponentType.Create<Firing>());
        }

        [Inject]
        private FiringBarrier _barrier;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new FiringJob
            {
                EntityCommandBuffer = _barrier.CreateCommandBuffer(),
                Weapons = _componentGroup.GetComponentDataArray<Weapon>(),
                Positions = _componentGroup.GetComponentDataArray<Position>(),
                Rotations = _componentGroup.GetComponentDataArray<Rotation>()
            }.Schedule(_componentGroup.CalculateLength(), 64, inputDeps);
        }

        private struct FiringJob : IJobParallelFor
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public ComponentDataArray<Weapon> Weapons;
            public ComponentDataArray<Position> Positions;
            public ComponentDataArray<Rotation> Rotations;

            public void Execute(int index)
            {
                EntityCommandBuffer.CreateEntity();
                EntityCommandBuffer.AddSharedComponent(BootStrap.BulletRenderer);
                EntityCommandBuffer.AddComponent(new TransformMatrix());
                EntityCommandBuffer.AddSharedComponent(new MoveForward());
                EntityCommandBuffer.AddComponent(new MoveSpeed { speed = Weapons[index].BulletVelocity });
                EntityCommandBuffer.AddComponent(Positions[index]);
                EntityCommandBuffer.AddComponent(Rotations[index]);
            }
        }

        private class FiringBarrier : BarrierSystem
        {
        }
    }
}