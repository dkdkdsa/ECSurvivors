using Unity.Entities;

namespace Game.ECS
{
    [System.Serializable]
    public struct BulletSetup
    {
        public float damage;
        public float lifeTime;
        public float moveSpeed;
        public float size;
        public int penetCount; //잔여 관통 횟수
    }

    public struct AutoAttack : IComponentData
    {
        public Entity prefab;
        public float radius;
        public float attackInterval;
        public float currentInterval;
        public BulletSetup setup;
    }
}