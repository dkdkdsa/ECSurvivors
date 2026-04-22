using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.ECS
{
    [BurstCompile]
    [UpdateAfter(typeof(RigidbodySystem))]
    public partial struct FlipSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FlipJob().ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct FlipJob : IJobEntity
        {
            public void Execute(
                in RigidbodyComponent rigid,
                ref BaseRotation baseRot)
            {
                if (rigid.velocity.x == 0f) return;

                baseRot.Value = rigid.velocity.x > 0
                    ? quaternion.RotateY(math.PI)
                    : quaternion.identity;
            }
        }
    }
}