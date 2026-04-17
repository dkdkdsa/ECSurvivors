using Unity.Entities;
using Unity.Mathematics;

public struct DropEvent : IComponentData
{
    public Entity prefab;
    public int dropCount;
    public float3 position;
}