using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial struct DropSystem : ISystem
{
    private Random _random;

    public void OnCreate(ref SystemState state)
    {
        _random = new Random(123);
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach(var (evt, entity) in SystemAPI.Query<
            RefRO<DropEvent>>().WithEntityAccess())
        {
            for(int i = 0; i < evt.ValueRO.dropCount; i++)
            {
                var dir = _random.NextFloat3Direction();
                var pos = evt.ValueRO.position + dir;

                var exp = ecb.Instantiate(evt.ValueRO.prefab);
                ecb.SetComponent(exp, LocalTransform.FromPosition(pos));
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}