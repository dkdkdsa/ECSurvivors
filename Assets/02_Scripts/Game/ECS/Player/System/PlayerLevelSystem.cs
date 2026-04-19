using Unity.Burst;
using Unity.Entities;

namespace Game.ECS
{
    public struct PlayerInfo : IComponentData
    {
        public int level;
        public int exp;
        public int needLevelUp;
    }

    [BurstCompile]
    public partial struct PlayerLevelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<PlayerInfo>(new PlayerInfo
            {
                needLevelUp = int.MaxValue,
            });
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var info in SystemAPI.Query<RefRW<PlayerInfo>>())
            {
                while (info.ValueRW.exp >= info.ValueRW.needLevelUp)
                {
                    info.ValueRW.exp -= info.ValueRW.needLevelUp;
                    info.ValueRW.level += 1;
                    info.ValueRW.needLevelUp *= 2;
                }
            }
        }
    }
}