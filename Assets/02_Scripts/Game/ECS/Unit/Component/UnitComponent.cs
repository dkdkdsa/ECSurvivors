using Unity.Entities;

namespace Game.ECS
{
    public struct UnitComponent : IComponentData
    {
        public float maxHP;
        public float currentHP;
        public float moveSpeed;
    }

}