using Unity.Entities;
using Unity.Mathematics;

public struct BoxColliderComponent : IComponentData
{
    public float2 size;
    public float2 offset;
    public bool isStatic;
    public bool isTrigger;
}