using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyMoveSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerPosition>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var pos = SystemAPI.GetSingleton<PlayerPosition>().Value;

        var job = new EnemyMoveJob
        {
            playerPos = pos,
        };

        job.ScheduleParallel();
    }

    [BurstCompile]
    private partial struct EnemyMoveJob : IJobEntity
    {
        [ReadOnly] public float3 playerPos;
        private void Execute(
            in EnemyTag tag, 
            in UnitComponent unit, 
            in LocalTransform transform, 
            ref RigidbodyComponent rigid)
        {
            var dir = math.normalize(playerPos - transform.Position);

            rigid.velocity = dir * unit.moveSpeed;
        }
    }
}