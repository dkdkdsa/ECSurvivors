using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Game.ECS
{
    [BurstCompile]
    public partial struct EnemySpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemySpawner>();
            state.RequireForUpdate<PlayerPosition>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spawnerRW = SystemAPI.GetSingletonRW<EnemySpawner>();
            ref var spawner = ref spawnerRW.ValueRW;

            spawner.timer -= SystemAPI.Time.DeltaTime;
            if (spawner.timer > 0f) return;

            spawner.timer = spawner.tick;

            var playerPos = SystemAPI.GetSingleton<PlayerPosition>().Value;

            int count = spawner.spawnPerTick;
            var entities = state.EntityManager.Instantiate(
                spawner.prefab, count, Allocator.Temp);

            for (int i = 0; i < count; i++)
            {
                var angle = spawner.random.NextFloat(0f, math.PI * 2f);
                var offset = new float3(
                    math.cos(angle) * spawner.radius,
                    math.sin(angle) * spawner.radius,
                    0);

                state.EntityManager.SetComponentData(
                    entities[i],
                    LocalTransform.FromPosition(playerPos + offset));
            }

            entities.Dispose();
        }
    }
}