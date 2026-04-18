using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Game.ECS
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    [BurstCompile]
    public partial struct ExpDestroySystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var info = SystemAPI.GetSingleton<PlayerInfo>();


            foreach (var (_, buffer, entity) in SystemAPI.Query<
                RefRO<ExpTag>,
                DynamicBuffer<CollisionEvent>>().WithEntityAccess())
            {
                foreach (var item in buffer)
                {
                    if (SystemAPI.HasComponent<PlayerTag>(item.other))
                    {
                        info.exp++;
                        ecb.DestroyEntity(entity);
                        break;
                    }
                }
            }

            SystemAPI.SetSingleton<PlayerInfo>(info);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}