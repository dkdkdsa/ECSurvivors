using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Game.ECS
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        public GameObject enemyPrefab;
        public float radius;
        public float tick;
        public int spawnPerTick;
        public uint randomSeed;

        public class Baker : Baker<EnemySpawnerAuthoring>
        {
            public override void Bake(EnemySpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new EnemySpawner
                {
                    prefab = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic),
                    radius = authoring.radius,
                    tick = authoring.tick,
                    timer = 0f,
                    spawnPerTick = authoring.spawnPerTick,
                    random = Random.CreateFromIndex(authoring.randomSeed)
                });
            }
        }
    }
}