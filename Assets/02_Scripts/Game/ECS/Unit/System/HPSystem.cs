using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct HPSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach(var (unit, buffer, entity) in SystemAPI.Query<
            RefRW<UnitComponent>,
            DynamicBuffer<DamageEvent>>().WithEntityAccess())
        {
            foreach(var item in buffer)
            {
                unit.ValueRW.currentHP -= item.amount;
            }

            if(unit.ValueRO.currentHP <= 0)
            {
                ecb.DestroyEntity(entity);
            }

            buffer.Clear();
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}