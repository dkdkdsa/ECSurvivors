using Unity.Entities;

namespace Game.ECS
{
    public struct BulletEnforceRequest : IComponentData
    {
        public BulletEnforceData data;
        public byte pending; // 0 = 없음, 1 = 처리 대기
    }
}