using Unity.Entities;
using Random = Unity.Mathematics.Random;

namespace Game.ECS
{
    public struct EnemySpawner : IComponentData
    {
        public Entity prefab;
        public float radius;
        public float tick;
        public float timer;
        public int spawnPerTick;
        public Random random;
    }
}