using Unity.Entities;

namespace Game.ECS
{
    public struct DropTable : IComponentData
    {
        public Entity prefab;
        public int dropCount;
    }
}