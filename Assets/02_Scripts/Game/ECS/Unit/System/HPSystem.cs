using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct HPSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach(var (unit, buffer, trm, entity) in SystemAPI.Query<
            RefRW<UnitComponent>,
            DynamicBuffer<DamageEvent>,
            RefRO<LocalTransform>>().WithEntityAccess())
        {
            foreach(var item in buffer)
            {
                unit.ValueRW.currentHP -= item.amount;
            }

            if(unit.ValueRO.currentHP <= 0)
            {
                ecb.DestroyEntity(entity);

                if (SystemAPI.HasComponent<DropTable>(entity))
                {
                    var table = SystemAPI.GetComponent<DropTable>(entity);
                    var evt = new DropEvent
                    {
                        dropCount = table.dropCount,
                        prefab = table.prefab,
                        position = trm.ValueRO.Position
                    };

                    var e = ecb.CreateEntity();
                    ecb.AddComponent(e, evt);
                }
            }

            buffer.Clear();
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}