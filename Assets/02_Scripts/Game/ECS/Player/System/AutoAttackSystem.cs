using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(RigidbodySystem))]
[BurstCompile]
public partial struct AutoAttackSystem : ISystem
{
    private EntityQuery _enemyQuery;
    private EntityQuery _attackerQuery;
    private ComponentLookup<BulletComponent> _bulletLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _enemyQuery = SystemAPI.QueryBuilder()
            .WithAll<EnemyTag, LocalTransform>()
            .Build();

        _attackerQuery = SystemAPI.QueryBuilder()
            .WithAll<AutoAttack, LocalTransform>()
            .Build();

        _bulletLookup = state.GetComponentLookup<BulletComponent>(isReadOnly: true);

        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate(_attackerQuery);
        state.RequireForUpdate(_enemyQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        float dt = SystemAPI.Time.DeltaTime;

        var tickJob = new TickCooldownJob { dt = dt };
        state.Dependency = tickJob.ScheduleParallel(state.Dependency);

        var enemyTransforms = _enemyQuery.ToComponentDataListAsync<LocalTransform>(
            state.WorldUpdateAllocator, state.Dependency, out var enemyFetchHandle);

        _bulletLookup.Update(ref state);

        var ecb = SystemAPI
            .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        state.Dependency = new AttackJob
        {
            enemyTransforms = enemyTransforms.AsDeferredJobArray(),  // ← 여기
            bulletLookup = _bulletLookup,
            ecb = ecb
        }.ScheduleParallel(JobHandle.CombineDependencies(state.Dependency, enemyFetchHandle));
    }

    [BurstCompile]
    partial struct TickCooldownJob : IJobEntity
    {
        public float dt;

        public void Execute(ref AutoAttack attack)
        {
            if (attack.currentInterval > 0f)
                attack.currentInterval -= dt;
        }
    }

    [BurstCompile]
    partial struct AttackJob : IJobEntity
    {
        [ReadOnly] public NativeArray<LocalTransform> enemyTransforms;
        [ReadOnly] public ComponentLookup<BulletComponent> bulletLookup;
        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute(
            [ChunkIndexInQuery] int sortKey,
            ref AutoAttack attack,
            in LocalTransform transform)
        {
            if (attack.currentInterval > 0f) return;
            if (enemyTransforms.Length == 0) return;

            float2 myPos = transform.Position.xy;
            float radiusSq = attack.radius * attack.radius;

            int closestIdx = -1;
            float closestDistSq = radiusSq;

            for (int i = 0; i < enemyTransforms.Length; i++)
            {
                float2 diff = enemyTransforms[i].Position.xy - myPos;
                float distSq = math.lengthsq(diff);

                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closestIdx = i;
                }
            }

            if (closestIdx < 0) return;

            attack.currentInterval = attack.attackInterval;

            float2 targetXY = enemyTransforms[closestIdx].Position.xy;
            float2 dir2 = math.normalize(targetXY - myPos);

            var bulletData = bulletLookup[attack.prefab];
            bulletData.dir = new float3(dir2, 0f);
            bulletData.setup = attack.setup;
            bulletData.lifeTime = attack.setup.lifeTime;

            var bullet = ecb.Instantiate(sortKey, attack.prefab);
            ecb.SetComponent(sortKey, bullet,
                LocalTransform.FromPositionRotationScale(
                    transform.Position,
                    quaternion.identity,
                    bulletData.setup.size));
            ecb.SetComponent(sortKey, bullet, bulletData);
        }
    }
}