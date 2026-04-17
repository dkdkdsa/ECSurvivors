using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct ExpAttractSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerPosition>();
    }

    public void OnUpdate(ref SystemState state)
    {
        float2 playerPos = SystemAPI.GetSingleton<PlayerPosition>().Value.xy;
        float dt = SystemAPI.Time.DeltaTime;

        state.Dependency = new AttractJob
        {
            playerPos = playerPos,
            dt = dt
        }.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    [WithAll(typeof(ExpTag))]
    partial struct AttractJob : IJobEntity
    {
        public float2 playerPos;
        public float dt;

        private const float FollowStrength = 12f;
        private const float MaxSpeed = 25f;
        private const float SlowDownDistance = 1.5f;

        public void Execute(
            ref RigidbodyComponent rb,
            in LocalTransform transform)
        {
            float2 pos = transform.Position.xy;
            float2 toPlayer = playerPos - pos;
            float distSq = math.lengthsq(toPlayer);

            if (distSq < 0.0001f)
            {
                rb.velocity = new float3(0f, 0f, rb.velocity.z);
                return;
            }

            float dist = math.sqrt(distSq);
            float2 dir = toPlayer / dist;
            float2 vel2 = rb.velocity.xy;

            float targetSpeed = MaxSpeed;

            if (dist < SlowDownDistance)
            {
                float ratio = dist / SlowDownDistance;
                targetSpeed *= math.saturate(ratio);
            }

            float2 desiredVel = dir * targetSpeed;

            float t = math.saturate(FollowStrength * dt);
            vel2 = math.lerp(vel2, desiredVel, t);

            rb.velocity = new float3(vel2, rb.velocity.z);
        }
    }
}