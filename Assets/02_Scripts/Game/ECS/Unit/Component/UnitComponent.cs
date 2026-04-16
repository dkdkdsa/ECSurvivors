using Unity.Entities;

public struct UnitComponent : IComponentData
{
    public float maxHP;
    public float currentHP;
    public float moveSpeed;
}
