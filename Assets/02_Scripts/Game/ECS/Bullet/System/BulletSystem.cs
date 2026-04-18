using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Game.ECS
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    public partial struct BulletSystem : ISystem
    {
        private BufferLookup<DamageEvent> _damageEventLookup;
        private ComponentLookup<EnemyTag> _enemyTagLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _damageEventLookup = state.GetBufferLookup<DamageEvent>(true);
            _enemyTagLookup = state.GetComponentLookup<EnemyTag>(true);

            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _damageEventLookup.Update(ref state);
            _enemyTagLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new BulletUpdateJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                damageEventLookup = _damageEventLookup,
                enemyTagLookup = _enemyTagLookup,
                ecb = ecb
            }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct BulletUpdateJob : IJobEntity
    {
        public float deltaTime;

        [ReadOnly] public BufferLookup<DamageEvent> damageEventLookup;
        [ReadOnly] public ComponentLookup<EnemyTag> enemyTagLookup;

        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute(
            [ChunkIndexInQuery] int sortKey,
            Entity entity,
            ref BulletComponent bullet,
            ref RigidbodyComponent rigid,
            in DynamicBuffer<CollisionEvent> hitBuffer)
        {
            rigid.velocity = bullet.dir * bullet.setup.moveSpeed;
            bullet.lifeTime -= deltaTime;

            if (bullet.lifeTime <= 0f)
            {
                ecb.DestroyEntity(sortKey, entity);
                return;
            }

            for (int i = 0; i < hitBuffer.Length; i++)
            {
                var target = hitBuffer[i].other;

                if (!damageEventLookup.HasBuffer(target) || !enemyTagLookup.HasComponent(target))
                    continue;

                ecb.AppendToBuffer(sortKey, target, new DamageEvent
                {
                    amount = bullet.setup.damage
                });

                if (bullet.setup.penetCount > 0)
                {
                    bullet.setup.penetCount--;
                }
                else
                {
                    ecb.DestroyEntity(sortKey, entity);
                    break;
                }

            }
        }
    }
}