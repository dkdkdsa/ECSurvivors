using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollisionSystem))]
[BurstCompile]
public partial struct EnemyAttackSystem : ISystem
{
    private BufferLookup<DamageEvent> _damageLookup;
    private ComponentLookup<PlayerTag> _playerTagLookup;

    public void OnCreate(ref SystemState state)
    {
        _damageLookup = state.GetBufferLookup<DamageEvent>();
        _playerTagLookup = state.GetComponentLookup<PlayerTag>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _damageLookup.Update(ref state);
        _playerTagLookup.Update(ref state);

        state.Dependency = new EnemyAttackJob
        {
            damageLookup = _damageLookup,
            playerTagLookup = _playerTagLookup
        }.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct EnemyAttackJob : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        public BufferLookup<DamageEvent> damageLookup;

        [ReadOnly] public ComponentLookup<PlayerTag> playerTagLookup;

        public void Execute(in DynamicBuffer<CollisionEvent> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                var evt = buffer[i];

                if (evt.isTrigger) continue;
                if (!playerTagLookup.HasComponent(evt.other)) continue;
                if (!damageLookup.HasBuffer(evt.other)) continue;

                var targetBuffer = damageLookup[evt.other];
                targetBuffer.Add(new DamageEvent { amount = 1f });
            }
        }
    }
}