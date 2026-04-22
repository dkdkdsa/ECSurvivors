using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.ECS
{
    [BurstCompile]
    public partial struct PlayerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var input = SystemAPI.GetSingleton<InputData>();
            var dt = SystemAPI.Time.DeltaTime;
            var move = new float3(input.Move.x, input.Move.y, 0);

            foreach (var (rigid, unit, rot, trm) in
                SystemAPI.Query<
                RefRW<RigidbodyComponent>,
                RefRO<UnitComponent>,
                RefRO<BaseRotation>,
                RefRW<LocalTransform>>().WithAll<PlayerTag>())
            {
                rigid.ValueRW.velocity = move * unit.ValueRO.moveSpeed;
                trm.ValueRW.Rotation = rot.ValueRO.Value;
            }
        }
    }
}