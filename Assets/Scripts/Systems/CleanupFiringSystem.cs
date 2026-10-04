using Assets.Scripts.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    public class CleanupFiringSystem : JobComponentSystem
    {
        private struct CleanUpFiringJob : IJobParallelFor
        {
            [ReadOnly]
            public EntityArray Entities;
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public float CurrentTime;
            public ComponentDataArray<Firing> Firings;

            public void Execute(int index)
            {
                // If the last time it was fired more than .5 seconds ago, remove the firing component from the entity.
                if (CurrentTime - Firings[index].FiredAt < 0.5f) return;
                EntityCommandBuffer.RemoveComponent<Firing>(Entities[index]);
            }
        }

        public struct Data
        {
            public readonly int Length;
            public EntityArray Entities;
            public ComponentDataArray<Firing> FiringEntities;
        }

        [Inject]
        private Data _data;

        [Inject]
        private CleanupFiringBarrier _barrier;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new CleanUpFiringJob
            {
                Entities =  _data.Entities,
                EntityCommandBuffer = _barrier.CreateCommandBuffer(),
                CurrentTime = Time.time,
                Firings = _data.FiringEntities
            }.Schedule(_data.Length, 64, inputDeps);
        }
    }

    public class CleanupFiringBarrier : BarrierSystem
    {
    }
}