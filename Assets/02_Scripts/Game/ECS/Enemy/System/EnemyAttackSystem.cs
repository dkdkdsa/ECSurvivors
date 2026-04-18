using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Game.ECS
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(CollisionSystem))]
    [BurstCompile]
    public partial struct EnemyAttackSystem : ISystem
    {
        private const float AttackDamage = 1f;

        private ComponentLookup<PlayerTag> _playerTagLookup;
        private BufferLookup<DamageEvent> _damageBufferLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _playerTagLookup = state.GetComponentLookup<PlayerTag>(true);
            _damageBufferLookup = state.GetBufferLookup<DamageEvent>(true);

            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _playerTagLookup.Update(ref state);
            _damageBufferLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new EnemyAttackJob
            {
                damage = AttackDamage,
                playerTagLookup = _playerTagLookup,
                damageBufferLookup = _damageBufferLookup,
                ecb = ecb
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(EnemyTag))]
        private partial struct EnemyAttackJob : IJobEntity
        {
            public float damage;

            [ReadOnly] public ComponentLookup<PlayerTag> playerTagLookup;
            [ReadOnly] public BufferLookup<DamageEvent> damageBufferLookup;

            public EntityCommandBuffer.ParallelWriter ecb;

            private void Execute(
                [ChunkIndexInQuery] int sortKey,
                in DynamicBuffer<CollisionEvent> buffer)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    var evt = buffer[i];

                    if (evt.isTrigger) continue;
                    if (!playerTagLookup.HasComponent(evt.other)) continue;
                    if (!damageBufferLookup.HasBuffer(evt.other)) continue;

                    ecb.AppendToBuffer(sortKey, evt.other, new DamageEvent
                    {
                        amount = damage
                    });
                }
            }
        }
    }
}