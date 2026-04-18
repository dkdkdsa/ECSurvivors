using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct RigidbodyComponent : IComponentData
    {
        public float3 velocity;
    }
}