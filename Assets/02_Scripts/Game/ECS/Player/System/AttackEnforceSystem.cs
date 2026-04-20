using Unity.Burst;
using Unity.Entities;

namespace Game.ECS
{
    [BurstCompile]
    public partial struct AttackEnforceSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonRW<BulletEnforceRequest>(out var requestRW))
                return;

            if (requestRW.ValueRO.pending == 0)
                return;

            BulletEnforceData data = requestRW.ValueRO.data;

            foreach (var item in SystemAPI.Query<RefRW<AutoAttack>>())
            {
                switch (data.type)
                {
                    case EnforceType.BulletSize:
                        item.ValueRW.setup.size += data.value;
                        break;
                    case EnforceType.PenetCount:
                        item.ValueRW.setup.penetCount += (int)data.value;
                        break;
                    case EnforceType.BulletSpeed:
                        item.ValueRW.setup.moveSpeed += data.value;
                        break;
                    case EnforceType.Damage:
                        item.ValueRW.setup.damage += data.value;
                        break;
                    case EnforceType.LifeTime:
                        item.ValueRW.setup.lifeTime += data.value;
                        break;
                }
            }

            requestRW.ValueRW.pending = 0;
        }
    }
}