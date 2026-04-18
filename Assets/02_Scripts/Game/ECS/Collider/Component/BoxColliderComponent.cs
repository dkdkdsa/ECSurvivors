using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct BoxColliderComponent : IComponentData
    {
        public float2 size;
        public float2 offset;
        public bool isStatic;
        public bool isTrigger;
    }
}