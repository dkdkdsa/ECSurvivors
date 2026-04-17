using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(TransformSystemGroup))]
[UpdateBefore(typeof(LocalToWorldSystem))]
[BurstCompile]
public partial struct EnemyAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float elapsed = (float)SystemAPI.Time.ElapsedTime;

        state.Dependency = new WaddleJob
        {
            elapsed = elapsed
        }.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    partial struct WaddleJob : IJobEntity
    {
        public float elapsed;

        private const float Amplitude = 0.25f;
        private const float Frequency = 6f;

        public void Execute(
            ref LocalTransform transform,
            [EntityIndexInQuery] int entityIndex)
        {

            float phase = entityIndex * 0.37f;

            float angle = math.sin((elapsed + phase) * Frequency) * Amplitude;

            transform.Rotation = quaternion.RotateZ(angle);
        }
    }
}