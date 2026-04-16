using Unity.Entities;
using Unity.Mathematics;

public struct RigidbodyComponent : IComponentData
{
    public float3 velocity;
}