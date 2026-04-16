using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(CollisionSystem))]
public partial struct BulletSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var lookup = state.GetBufferLookup<DamageEvent>();

        foreach(var (bullet, rigid, hitBuffer, entity) in SystemAPI.Query<
            RefRW<BulletComponent>,
            RefRW<RigidbodyComponent>,
            DynamicBuffer<CollisionEvent>>().WithEntityAccess())
        {
            rigid.ValueRW.velocity = bullet.ValueRO.dir * bullet.ValueRO.moveSpeed;
            bullet.ValueRW.lifeTime -= SystemAPI.Time.DeltaTime;

            if (bullet.ValueRO.lifeTime <= 0f)
            {
                ecb.DestroyEntity(entity);
                continue;
            }

            foreach (var evt in hitBuffer)
            {
                var target = evt.other;

                if(state.EntityManager.HasBuffer<DamageEvent>(target) && state.EntityManager
                    .HasComponent<EnemyTag>(target))
                {
                    var buffer = lookup[target];
                    buffer.Add(new DamageEvent
                    {
                        amount = bullet.ValueRO.damage
                    });

                    ecb.DestroyEntity(target);

                    break;
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}