using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct RigidbodySystem : ISystem
{
    public const float FixedDT = 1f/60f;
    
    public void OnUpdate(ref SystemState state)
    {
        var job = new RigidbodyMoveJob();
        job.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct RigidbodyMoveJob : IJobEntity
    {
        private void Execute(in RigidbodyComponent rigid, ref LocalTransform transform)
        {
            var velocity = rigid.velocity;
            transform.Position += new float3(velocity.x, velocity.y, 0) * FixedDT;
        }
    }
}