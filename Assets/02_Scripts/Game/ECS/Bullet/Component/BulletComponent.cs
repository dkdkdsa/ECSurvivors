using Unity.Entities;
using Unity.Mathematics;

public struct BulletComponent : IComponentData
{
    public BulletSetup setup;
    public float lifeTime;
    public float3 dir;
}