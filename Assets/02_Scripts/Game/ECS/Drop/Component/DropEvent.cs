using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct DropEvent : IComponentData
    {
        public Entity prefab;
        public int dropCount;
        public float3 position;
    }
}