using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct PlayerPosition : IComponentData
    {
        public float3 Value;
    }
}