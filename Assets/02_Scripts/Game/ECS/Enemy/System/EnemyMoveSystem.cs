using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.ECS
{
    [BurstCompile]
    public partial struct EnemyMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerPosition>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var pos = SystemAPI.GetSingleton<PlayerPosition>().Value;

            var job = new EnemyMoveJob
            {
                playerPos = pos,
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct EnemyMoveJob : IJobEntity
        {
            [ReadOnly] public float3 playerPos;
            public void Execute(
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
}