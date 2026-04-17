using Unity.Entities;

[System.Serializable]
public struct BulletSetup
{
    public float damage;
    public float lifeTime;
    public float moveSpeed;
    public float size;
}

public struct AutoAttack : IComponentData
{
    public Entity prefab;
    public float radius;
    public float attackInterval;
    public float currentInterval;
    public BulletSetup setup;
}