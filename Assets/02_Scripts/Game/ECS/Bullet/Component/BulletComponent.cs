using Unity.Entities;
using Unity.Mathematics;

public struct BulletComponent : IComponentData
{
    public float damage;
    public float lifeTime;
    public float moveSpeed;
    public float3 dir;
}