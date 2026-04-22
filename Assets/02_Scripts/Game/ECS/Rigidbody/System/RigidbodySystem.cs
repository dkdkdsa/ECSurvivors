using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.ECS
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct RigidbodySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new RigidbodyMoveJob
            {
                dt = SystemAPI.Time.DeltaTime
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct RigidbodyMoveJob : IJobEntity
        {

            [ReadOnly] public float dt;

            public void Execute(in RigidbodyComponent rigid, ref LocalTransform transform)
            {
                var velocity = rigid.velocity;
                transform.Position += new float3(velocity.x, velocity.y, 0) * dt;
            }
        }
    }
}