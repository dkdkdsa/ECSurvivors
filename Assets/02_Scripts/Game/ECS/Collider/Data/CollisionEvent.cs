using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    [InternalBufferCapacity(8)]
    public struct CollisionEvent : IBufferElementData
    {
        public Entity other;
        public float2 normal;
        public float penetration;
        public bool isTrigger;
    }
}