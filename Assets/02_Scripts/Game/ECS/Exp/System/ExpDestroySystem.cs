using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(CollisionSystem))]
[BurstCompile]
public partial struct ExpDestroySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach(var (_, buffer, entity) in SystemAPI.Query<
            RefRO<ExpTag>,
            DynamicBuffer<CollisionEvent>>().WithEntityAccess())
        {
            foreach(var item in buffer)
            {
                if (SystemAPI.HasComponent<PlayerTag>(item.other))
                {
                    ecb.DestroyEntity(entity);
                    break;
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}