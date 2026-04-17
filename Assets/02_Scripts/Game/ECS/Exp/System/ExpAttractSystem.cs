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

        private const float Acceleration = 20f;
        private const float MaxSpeed = 25f;

        void Execute(
            ref RigidbodyComponent rb,
            in LocalTransform transform)
        {
            float2 pos = transform.Position.xy;
            float2 toPlayer = playerPos - pos;
            float distSq = math.lengthsq(toPlayer);

            if (distSq < 0.0001f) return;

            float2 dir = toPlayer / math.sqrt(distSq);
            float2 vel2 = rb.velocity.xy;

            vel2 += dir * Acceleration * dt;

            float speedSq = math.lengthsq(vel2);
            if (speedSq > MaxSpeed * MaxSpeed)
            {
                vel2 = vel2 / math.sqrt(speedSq) * MaxSpeed;
            }

            rb.velocity = new float3(vel2, rb.velocity.z);
        }
    }
}