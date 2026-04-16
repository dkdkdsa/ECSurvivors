using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct PlayerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var input = SystemAPI.GetSingleton<InputData>();
        var dt = SystemAPI.Time.DeltaTime;
        var move = new float3(input.Move.x, input.Move.y, 0);

        foreach (var (rigid, unit) in
            SystemAPI.Query<RefRW<RigidbodyComponent>, RefRO<UnitComponent>>().WithAll<PlayerTag>())
        {
            rigid.ValueRW.velocity = move * unit.ValueRO.moveSpeed;
        }
    }
}