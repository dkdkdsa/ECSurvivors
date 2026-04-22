using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlayerSystem))] // 이동 끝난 뒤 갱신
    [BurstCompile]
    public partial struct PlayerPositionSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<PlayerPosition>();
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var transform in
                SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerTag>())
            {
                SystemAPI.SetSingleton(new PlayerPosition
                {
                    Value = transform.ValueRO.Position
                });
            }
        }
    }
}