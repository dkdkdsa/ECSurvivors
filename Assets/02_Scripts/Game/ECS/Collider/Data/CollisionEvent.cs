using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(8)]
public struct CollisionEvent : IBufferElementData
{
    public Entity other;
    public float2 normal;
    public float penetration;
}