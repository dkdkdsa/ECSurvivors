using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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
            needLevelUp = 10,
        });
    }

    public void OnUpdate(ref SystemState state)
    {
        var info = SystemAPI.GetSingleton<PlayerInfo>();

        if(info.exp / info.needLevelUp > 0)
        {
            int remain = info.exp % info.needLevelUp;
            info.level = info.exp / info.needLevelUp;

            info.needLevelUp *= 2;
            info.exp = remain;
        }

        SystemAPI.SetSingleton<PlayerInfo>(info);
    }
}
