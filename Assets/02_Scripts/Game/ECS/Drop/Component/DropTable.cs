using Unity.Entities;

public struct DropTable : IComponentData
{
    public Entity prefab;
    public int dropCount;
}