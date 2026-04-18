using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct HPSystem : ISystem
    {
        private ComponentLookup<DropTable> dropTableLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            dropTableLookup = state.GetComponentLookup<DropTable>(isReadOnly: true);
        }

        public void OnUpdate(ref SystemState state)
        {
            dropTableLookup.Update(ref state);

            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged)
                .AsParallelWriter();

            state.Dependency = new HPJob
            {
                ecb = ecb,
                dropTableLookup = dropTableLookup
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        partial struct HPJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            [ReadOnly] public ComponentLookup<DropTable> dropTableLookup;

            public void Execute(
                [ChunkIndexInQuery] int sortKey,
                Entity entity,
                ref UnitComponent unit,
                ref DynamicBuffer<DamageEvent> buffer,
                in LocalTransform transform)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    unit.currentHP -= buffer[i].amount;
                }
                buffer.Clear();

                if (unit.currentHP > 0) return;

                if (dropTableLookup.HasComponent(entity))
                {
                    var table = dropTableLookup[entity];
                    var eventEntity = ecb.CreateEntity(sortKey);
                    ecb.AddComponent(sortKey, eventEntity, new DropEvent
                    {
                        dropCount = table.dropCount,
                        prefab = table.prefab,
                        position = transform.Position
                    });
                }

                ecb.DestroyEntity(sortKey, entity);
            }
        }
    }
}