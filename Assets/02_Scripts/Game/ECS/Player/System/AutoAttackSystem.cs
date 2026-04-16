using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(RigidbodySystem))]
public partial struct AutoAttackSystem : ISystem
{
    private EntityQuery enemyQuery;

    public void OnCreate(ref SystemState state)
    {
        enemyQuery = SystemAPI.QueryBuilder()
            .WithAll<EnemyTag, LocalTransform>()
            .Build();

        state.RequireForUpdate<AutoAttack>();
        state.RequireForUpdate(enemyQuery);
    }

    public void OnUpdate(ref SystemState state)
    {
        var enemyTransforms = enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        foreach (var (attack, transform) in
                 SystemAPI.Query<RefRO<AutoAttack>, RefRO<LocalTransform>>())
        {
            float3 myPos = transform.ValueRO.Position;
            float radiusSq = attack.ValueRO.radius * attack.ValueRO.radius;


            int closestIdx = -1;
            float closestDistSq = radiusSq;

            for (int i = 0; i < enemyTransforms.Length; i++)
            {
                float2 diff = enemyTransforms[i].Position.xy - myPos.xy;
                float distSq = math.lengthsq(diff);

                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closestIdx = i;
                }
            }

            if (closestIdx < 0) continue;

            float2 targetXY = enemyTransforms[closestIdx].Position.xy;
            float2 dir2 = math.normalize(targetXY - myPos.xy);

            var bulletData = SystemAPI.GetComponent<BulletComponent>(attack.ValueRO.prefab);
            bulletData.dir = new float3(dir2.x, dir2.y, 0f);

            var bullet = ecb.Instantiate(attack.ValueRO.prefab);
            ecb.SetComponent(bullet, LocalTransform.FromPosition(myPos));
            ecb.SetComponent(bullet, bulletData);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        enemyTransforms.Dispose();
    }
}