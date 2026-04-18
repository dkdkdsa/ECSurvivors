using Unity.Entities;
using Unity.Mathematics;

namespace Game.ECS
{
    public struct InputData : IComponentData
    {
        public float2 Move;
    }
}