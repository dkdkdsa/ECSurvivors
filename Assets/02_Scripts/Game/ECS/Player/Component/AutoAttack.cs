using Unity.Entities;

public struct AutoAttack : IComponentData
{
    public Entity prefab;
    public float radius;
}